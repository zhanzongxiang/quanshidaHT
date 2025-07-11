<template>
	<div class="login-container flex">
		<div class="login-left flex-margin">
			<div class="login-left-logo">
				<img :src="getThemeConfig.logoUrl" />
				<div class="login-left-logo-text">
					<span>{{ getThemeConfig.globalViceTitle }}</span>
					<span class="login-left-logo-text-msg">{{ getThemeConfig.globalViceTitleMsg }}</span>
				</div>
			</div>
			<el-carousel height="500px" class="login-carousel">
				<el-carousel-item>
					<img :src="loginIconTwo" class="login-icon-group-icon" />
				</el-carousel-item>
				<el-carousel-item>
					<img :src="loginIconTwo1" class="login-icon-group-icon" />
				</el-carousel-item>
				<el-carousel-item>
					<img :src="loginIconTwo2" class="login-icon-group-icon" />
				</el-carousel-item>
			</el-carousel>
		</div>
		<div class="login-right flex">
			<div class="login-right-warp flex-margin">
				<span class="login-right-warp-one"></span>
				<span class="login-right-warp-two"></span>
				<div class="login-right-warp-main">
					<div class="login-right-warp-main-title">{{ getThemeConfig.globalTitle }}</div>
					<div class="login-right-warp-main-form">
						<div v-if="!state.isScan">
							<el-tabs v-model="state.tabsActiveName">
								<el-tab-pane :label="$t('message.label.label1')" name="account" v-if="state.tabsActiveName != 'register'">
									<Account :tenant-info="tenantInfo" />
								</el-tab-pane>
								<el-tab-pane :label="$t('message.label.label2')" name="mobile" v-if="state.tabsActiveName != 'register'">
									<Mobile :tenant-info="tenantInfo" />
								</el-tab-pane>
								<el-tab-pane :label="$t('message.label.label3')" name="register" v-if="state.tabsActiveName == 'register'">
									<Register :tenant-info="tenantInfo" @goLogin="() => state.tabsActiveName = 'account'" />
								</el-tab-pane>
							</el-tabs>
						</div>
						<Scan v-if="state.isScan" :tenant-info="tenantInfo" />
						<div class="login-content-main-scan" @click="state.isScan = !state.isScan">
							<i class="iconfont" :class="state.isScan ? 'icon-diannao1' : 'icon-barcode-qr'"></i>
							<div class="login-content-main-scan-delta"></div>
						</div>
						<div class="login-content-main-left" v-if="getThemeConfig.registration">
							<template v-if="state.tabsActiveName != 'register'">
								{{$t('message.label.label4')}}<el-link class="login-content-main-left-register" @click="() => state.tabsActiveName = 'register'">{{$t('message.label.label5')}}</el-link>
							</template>
							<template v-else>
								{{$t('message.label.label6')}}<el-link class="login-content-main-left-register" @click="() => state.tabsActiveName = 'account'">{{$t('message.label.label7')}}</el-link>
							</template>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div class="copyright" :class="[getThemeConfig.icp ? 'mb25' : 'mt5']">{{ getThemeConfig.copyright }}</div>
		<div v-if="getThemeConfig.icp" class="icp mt5" onselect="false">
			<el-link :href="getThemeConfig.icpUrl" target="_blank">{{ getThemeConfig.icp }}</el-link>
		</div>
	</div>
</template>

<script setup lang="ts" name="loginIndex">
import { defineAsyncComponent, onMounted, reactive, computed, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import loginIconTwo from '/@/assets/login-icon-two.svg';
import loginIconTwo1 from '/@/assets/login-icon-two1.svg';
import loginIconTwo2 from '/@/assets/login-icon-two2.svg';
import {getAPI} from '/@/utils/axios-utils';
import {SysTenantApi} from '/@/api-services';
import {useRoute} from 'vue-router';
import {Local} from '/@/utils/storage';

// 引入组件
const Register = defineAsyncComponent(() => import('./component/register.vue'));
const Account = defineAsyncComponent(() => import('./component/account.vue'));
const Mobile = defineAsyncComponent(() => import('./component/mobile.vue'));
const Scan = defineAsyncComponent(() => import('./component/scan.vue'));

const route = useRoute();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const tenantInfo = ref({
	id: undefined as number | undefined,
	list: [],
});

const state = reactive({
	tabsActiveName: 'account',
	isScan: false,
});

// 获取布局配置信息
const getThemeConfig = computed(() => {
	return themeConfig.value;
});

// 页面加载时
onMounted(async () => {
	// 地址栏存在wayid参数时，默认切换到注册界面
	if (route.query.wayid != undefined) state.tabsActiveName = 'register';
	await getTenantInfo();
	NextLoading.done();
});

// 获取租户信息
const getTenantInfo = async () => {
	if (themeConfig.value.hideTenantForLogin) {
		return tenantInfo.value;
	}
	const host = location.host.toLowerCase();
	tenantInfo.value.list = await getAPI(SysTenantApi).apiSysTenantListGet().then(res => res.data.result ?? null);
	const tenant = tenantInfo.value.list.find((item: any) => !item.host && item.host === host) as any;
	if (tenant?.value) tenantInfo.value.id = parseInt(tenant?.value);
	return tenantInfo.value;
}
</script>

<style scoped lang="scss">
.login-container {
	height: 100%;
	background-color: rgba(53, 62, 84);
	.login-left {
		width: 50%;
		height: 100%;
		float: left;
		justify-content: center;
		.login-carousel {
			position: relative;
			top: 50%;
			transform: translateY(-50%);
		}
		.login-left-logo {
			display: flex;
			align-items: center;
			position: absolute;
			top: 30px;
			left: 30px;
			z-index: 1;
			animation: logoAnimation 0.3s ease;
			img {
				// width: 100px;
				height: 64px;
			}
			.login-left-logo-text {
				display: flex;
				flex-direction: column;
				span {
					margin-left: 20px;
					font-size: 28px;
					font-weight: 700;
					color: var(--el-color-white);
				}
				.login-left-logo-text-msg {
					padding-top: 5px;
					font-size: 14px;
					color: var(--el-color-white);
				}
			}
		}
		.login-icon-group-icon {
			width: 85%;
			height: 85%;
			position: absolute;
			left: 10%;
			top: 50%;
			transform: translateY(-50%) translate3d(0, 0, 0);
		}
	}
	.login-right {
		width: 50%;
		float: right;
		background: var(--el-color-white);
		background-image: url('../../assets/bg.svg');
		background-size: 100% auto;
		background-position: 50% calc(50% - 15px);
		background-attachment: fixed;
		background-repeat: no-repeat;
		.login-right-warp {
			border: 1px solid var(--el-color-primary-light-3);
			border-radius: 3px;
			height: 600px;
			position: relative;
			overflow: hidden;
			background-color: var(--el-color-white);
			.login-right-warp-one,
			.login-right-warp-two {
				position: absolute;
				display: block;
				width: inherit;
				height: inherit;
				&::before,
				&::after {
					content: '';
					position: absolute;
					z-index: 1;
				}
			}
			.login-right-warp-one {
				&::before {
					filter: hue-rotate(0deg);
					top: 0px;
					left: 0;
					width: 100%;
					height: 1px;
					background: linear-gradient(90deg, transparent, var(--el-color-primary));
					animation: loginLeft 3s linear infinite;
				}
				&::after {
					filter: hue-rotate(0deg);
					top: -100%;
					right: 2px;
					width: 1px;
					height: 100%;
					background: linear-gradient(180deg, transparent, var(--el-color-primary));
					animation: loginTop 3s linear infinite;
					animation-delay: 0.7s;
				}
			}
			.login-right-warp-two {
				&::before {
					filter: hue-rotate(0deg);
					bottom: 2px;
					right: -100%;
					width: 100%;
					height: 1px;
					background: linear-gradient(270deg, transparent, var(--el-color-primary));
					animation: loginRight 3s linear infinite;
					animation-delay: 1.4s;
				}
				&::after {
					filter: hue-rotate(0deg);
					bottom: -100%;
					left: 0px;
					width: 1px;
					height: 100%;
					background: linear-gradient(360deg, transparent, var(--el-color-primary));
					animation: loginBottom 3s linear infinite;
					animation-delay: 2.1s;
				}
			}
			.login-right-warp-main {
				display: flex;
				flex-direction: column;
				height: 100%;
				.login-right-warp-main-title {
					height: 130px;
					line-height: 130px;
					font-size: 32px;
					font-weight: 800;
					text-align: center;
					//letter-spacing: 3px;
					animation: logoAnimation 0.3s ease;
					animation-delay: 0.3s;
					color: var(--el-color-primary);
					user-select: none;
				}
				.login-right-warp-main-form {
					flex: 1;
					padding: 0 50px 50px;
					.login-content-main-scan {
						position: absolute;
						top: 0;
						right: 0;
						width: 50px;
						height: 50px;
						overflow: hidden;
						cursor: pointer;
						transition: all ease 0.3s;
						color: var(--el-color-primary);
						&-delta {
							position: absolute;
							width: 35px;
							height: 70px;
							z-index: 2;
							top: 2px;
							right: 21px;
							background: var(--el-color-white);
							transform: rotate(-45deg);
						}
						&:hover {
							opacity: 1;
							transition: all ease 0.3s;
							color: var(--el-color-primary) !important;
						}
						i {
							width: 47px;
							height: 50px;
							display: inline-block;
							font-size: 48px;
							position: absolute;
							right: 1px;
							top: 0px;
						}
					}
					.login-content-main-left {
						position: absolute;
						top: 10px;
						left: 10px;
						width: 150px;
						height: 50px;
						overflow: hidden;
						cursor: pointer;
						transition: all ease 0.3s;
						user-select: none;
						.login-content-main-left-register {
							top: -1.5px;
							color: var(--el-color-primary);
						}
					}
				}
			}
		}
		/* 在这里可以添加一个伪元素来覆盖原内容，实现磨砂效果 */
		.login-right-warp::before {
			content: '';
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			background-color: rgba(255, 255, 255, 1);
			filter: blur(4px); /* 调整模糊半径以改变磨砂效果强度 */
			z-index: 1;
		}

		/* 保持原有内容可见，放置在伪元素下方 */
		.login-right-warp > * {
			position: absolute;
			z-index: 2;
		}
	}
}
.copyright,
.icp {
	position: absolute;
	bottom: 2%;
	transform: translateX(-50%);
	white-space: nowrap;
}
@media screen and (min-width: 1200px) {
	.login-right-warp {
		width: 500px;
	}
	.copyright,
	.icp {
		left: 75%;
		color: var(--el-text-color-secondary);
	}
	.icp {
		.el-link {
			color: var(--el-text-color-secondary);
		}
	}
}
@media screen and (max-width: 1200px) {
	.copyright,
	.icp {
		left: 50%;
		color: var(--el-color-white);
	}
	.icp {
		.el-link {
			color: var(--el-color-white);
		}
	}
}
@media screen and (max-width: 580px) {
	.copyright,
	.icp {
		left: 50%;
		color: var(--el-text-color-secondary);
	}
	.icp {
		.el-link {
			color: var(--el-text-color-secondary);
		}
	}
}
</style>
