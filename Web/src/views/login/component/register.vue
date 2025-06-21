<template>
	<el-tooltip :visible="state.capsLockVisible" effect="light" :content="$t('message.account.placeholder5')" placement="top">
		<el-form ref="ruleFormRef" :model="state.ruleForm" size="large" :rules="state.rules" class="login-content-form">
			<el-form-item class="login-animation2" prop="tenantId" clearable v-if="!props.tenantInfo.id && !themeConfig.hideTenantForLogin">
				<el-select v-model="state.ruleForm.tenantId" :placeholder="$t('message.register.placeholder1')" style="width: 100%" filterable>
					<template #prefix>
						<i class="iconfont icon-shuxingtu el-input__icon"></i>
					</template>
					<el-option :value="item.value" :label="item.label" v-for="(item, index) in tenantInfo.list" :key="index" />
				</el-select>
			</el-form-item>
			<el-form-item class="login-animation1" prop="phone" clearable>
				<el-input text :placeholder="$t('message.register.placeholder2')" v-model="state.ruleForm.phone" clearable autocomplete="off">
					<template #prefix>
						<i class="iconfont icon-dianhua el-input__icon"></i>
					</template>
				</el-input>
			</el-form-item>
			<el-form-item class="login-animation1" prop="account" clearable>
				<el-input ref="accountRef" text :placeholder="$t('message.register.placeholder3')" v-model="state.ruleForm.account" clearable autocomplete="off" @keyup.enter.native="handleRegister">
					<template #prefix>
						<el-icon>
							<ele-User />
						</el-icon>
					</template>
				</el-input>
			</el-form-item>
			<el-form-item class="login-animation1" prop="realName" clearable>
				<el-input ref="accountRef" text :placeholder="$t('message.register.placeholder4')" v-model="state.ruleForm.realName" clearable autocomplete="off" @keyup.enter.native="handleRegister">
					<template #prefix>
						<el-icon>
							<ele-User />
						</el-icon>
					</template>
				</el-input>
			</el-form-item>
			<el-form-item class="login-animation3" prop="code">
				<el-col :span="15">
					<el-input
						ref="codeRef"
						text
						maxlength="4"
						:placeholder="$t('message.register.placeholder5')"
						v-model="state.ruleForm.code"
						clearable
						autocomplete="off"
						@keyup.enter.native="handleRegister"
					>
						<template #prefix>
							<el-icon>
								<ele-Position />
							</el-icon>
						</template>
					</el-input>
				</el-col>
				<el-col :span="1"></el-col>
				<el-col :span="8">
					<div :class="[state.expirySeconds > 0 ? 'login-content-code' : 'login-content-code-expired']" @click="getCaptcha">
						<img class="login-content-code-img" width="130px" height="38px" :src="state.captchaImage" style="cursor: pointer" />
					</div>
				</el-col>
			</el-form-item>
			<el-form-item class="login-animation4">
				<el-button type="primary" class="login-content-submit" round v-waves @click="handleRegister" :loading="state.loading.register">
					<span>{{ $t('message.register.btnText') }}</span>
				</el-button>
			</el-form-item>
			<div class="font12 mt30 login-animation4 login-msg">{{ $t('message.mobile.msgText') }}</div>
		</el-form>
	</el-tooltip>
	<div class="dialog-header">
		<el-dialog v-model="state.rotateVerifyVisible" :show-close="false">
			<DragVerifyImgRotate
				ref="dragRef"
				:imgsrc="state.rotateVerifyImg"
				v-model:isPassing="state.isPassRotate"
				:text="$t('message.account.placeholder6')"
				:successText="$t('message.account.placeholder7')"
				handlerIcon="fa fa-angle-double-right"
				successIcon="fa fa-hand-peace-o"
				@passcallback="passRotateVerify"
			/>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="loginAccount">
import {reactive, ref, onMounted, defineAsyncComponent, onUnmounted, watch} from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, InputInstance } from 'element-plus';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { feature, getAPI } from '/@/utils/axios-utils';
import { SysAuthApi } from '/@/api-services/api';
import { useThemeConfig } from '/@/stores/themeConfig';
import {sm2} from 'sm-crypto-v2';

const props = defineProps({
	tenantInfo: {
		required: true,
		type: Object,
	},
});

// 旋转图片滑块组件
// import verifyImg from '/@/assets/logo-mini.svg';
const DragVerifyImgRotate = defineAsyncComponent(() => import('/@/components/dragVerify/dragVerifyImgRotate.vue'));

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const { t } = useI18n();
const route = useRoute();

const ruleFormRef = ref();
const accountRef = ref<InputInstance>();
const codeRef = ref<InputInstance>();

const emits = defineEmits(['reload']);
const dragRef: any = ref(null);
const state = reactive({
	ruleForm: {
		tenantId: props.tenantInfo.id,
		account: undefined,
		password: undefined,
		realName: undefined,
		phone: undefined,
		wayId: undefined,
		code: '',
		codeId: 0,
	},
	rules: {
		account: [{ required: true, message: t('message.register.placeholder3'), trigger: 'blur' }],
		realName: [{ required: true, message: t('message.register.placeholder4'), trigger: 'blur' }],
		phone: [{ required: true, message: t('message.register.placeholder2'), trigger: 'blur' }],
		code: [{ required: true, message: t('message.register.placeholder5'), trigger: 'blur' }],
	},
	loading: {
		register: false,
	},
	captchaImage: '',
	rotateVerifyVisible: false,
	rotateVerifyImg: themeConfig.value.logoUrl,
	secondVerEnabled: false,
	isPassRotate: false,
	capsLockVisible: false,
	expirySeconds: 60, // 验证码过期时间
});

// 验证码过期计时器
let timer: any = null;

// 页面初始化
onMounted(async () => {
	// 默认尝试从地址栏获取wayid注册方案id
	if (route.query.wayid) state.ruleForm.wayId = route.query.wayid as any;
	watch(
		() => themeConfig.value.isLoaded,
		(isLoaded) => {
			if (isLoaded) {
				// 获取登录配置
				state.secondVerEnabled = themeConfig.value.secondVer ?? true;

				// 获取验证码
				getCaptcha();

				// 注册验证码过期计时器
				timer = setInterval(() => {
					if (state.expirySeconds > 0) state.expirySeconds -= 1;
				}, 1000);
			}
		},
		{ immediate: true }
	);

	// 检测大小写按键/CapsLK
	document.addEventListener('keyup', handleKeyPress);
});

// 页面卸载
onUnmounted(() => {
	// 销毁验证码过期计时器
	clearInterval(timer);
	timer = null;

	document.removeEventListener('keyup', handleKeyPress);
});

// 检测大小写按键
const handleKeyPress = (e: KeyboardEvent) => {
	state.capsLockVisible = e.getModifierState('CapsLock');
};

// 获取验证码
const getCaptcha = async () => {
	state.ruleForm.code = '';
	const res = await getAPI(SysAuthApi).apiSysAuthCaptchaGet().then(res => res.data.result);
	state.captchaImage = 'data:text/html;base64,' + res?.img;
	state.expirySeconds = res?.expirySeconds;
  state.ruleForm.codeId = res?.id;
};

// 注册
const onRegister = async () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return false;

		try {
			state.loading.register = true;

			const publicKey = window.__env__.VITE_SM_PUBLIC_KEY;
			const password = state.ruleForm.password ? sm2.doEncrypt(state.ruleForm.password, publicKey, 1) : undefined;

			state.ruleForm.tenantId ??= props.tenantInfo.id ?? props.tenantInfo.list[0]?.value ?? undefined;
			const [err, res] = await feature(getAPI(SysAuthApi).apiSysAuthUserRegistrationPost({...state.ruleForm, password: password } as any));

			if (res?.data?.code === 200) {
				const registerText = t('message.registerText');
				ElMessage.success(registerText);
				emits('goLogin');
				return;
			}

			if (err) {
				getCaptcha(); // 重新获取验证码
			} else if (res.data.type != 'success') {
				getCaptcha(); // 重新获取验证码
				ElMessage.error(t('message.register.placeholder6'));
			}
		} finally {
			state.loading.register = false;
		}
	});
};

// 打开旋转验证
const openRotateVerify = () => {
	state.rotateVerifyVisible = true;
	state.isPassRotate = false;
	dragRef.value?.reset();
};

// 通过旋转验证
const passRotateVerify = () => {
	state.rotateVerifyVisible = false;
	state.isPassRotate = true;
	onSignIn();
};

// 注册处理
const handleRegister = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return false;
		state.secondVerEnabled ? openRotateVerify() : onRegister();
	});
};
</script>

<style lang="scss" scoped>
.dialog-header {
	:deep(.el-dialog) {
		width: unset !important;

		.el-dialog__header {
			display: none;
		}

		.el-dialog__wrapper {
			position: absolute !important;
		}

		.v-modal {
			position: absolute !important;
		}
	}
}

.login-content-form {
	margin-top: 20px;

	@for $i from 0 through 4 {
		.login-animation#{$i} {
			opacity: 0;
			animation-name: error-num;
			animation-duration: 0.5s;
			animation-fill-mode: forwards;
			animation-delay: calc($i/10) + s;
		}
	}

	.login-content-password {
		display: inline-block;
		width: 20px;
		cursor: pointer;

		&:hover {
			color: #909399;
		}
	}

	.login-content-code {
		display: flex;
		align-items: center;
		justify-content: space-around;
		position: relative;

		.login-content-code-img {
			width: 100%;
			height: 40px;
			line-height: 40px;
			background-color: #ffffff;
			border: 1px solid rgb(220, 223, 230);
			cursor: pointer;
			transition: all ease 0.2s;
			border-radius: 4px;
			user-select: none;

			&:hover {
				border-color: #c0c4cc;
				transition: all ease 0.2s;
			}
		}
	}

	.login-content-code-expired {
		@extend .login-content-code;
		&::before {
			content: '验证码已过期';
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 4px;
			background-color: rgba(0, 0, 0, 0.5);
			color: #ffffff;
			text-align: center;
		}
	}

	.login-content-submit {
		width: 100%;
		letter-spacing: 2px;
		font-weight: 300;
		margin-top: 15px;
	}

	.login-msg {
		color: var(--el-text-color-placeholder);
	}
}
</style>
