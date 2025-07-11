import js from '@eslint/js';
import vue from 'eslint-plugin-vue';
import tseslint from '@typescript-eslint/eslint-plugin';
import vueParser from 'vue-eslint-parser';
import globals from 'globals';

export default [
	{
		ignores: [
			'**/*.sh',
			'**/node_modules',
			'**/lib',
			'**/*.md',
			'**/*.scss',
			'**/*.woff',
			'**/*.ttf',
			'**/.vscode',
			'**/.idea',
			'**/dist',
			'**/mock',
			'**/public',
			'**/bin',
			'**/build',
			'**/config',
			'**/index.html',
			'src/assets',
		],
	},
	js.configs.recommended,
	{
		plugins: {
			vue,
			'@typescript-eslint': tseslint,
		},

		languageOptions: {
			parser: vueParser,
			parserOptions: {
				parser: '@typescript-eslint/parser',
				ecmaVersion: 2022,
				sourceType: 'module',
			},
			globals: {
				...globals.browser,
				...globals.node,
			},
		},
		rules: {
			'@typescript-eslint/ban-ts-ignore': 'off',
			'@typescript-eslint/explicit-function-return-type': 'off',
			'@typescript-eslint/no-explicit-any': 'off',
			'@typescript-eslint/no-var-requires': 'off',
			'@typescript-eslint/no-empty-function': 'off',
			'@typescript-eslint/no-use-before-define': 'off',
			'@typescript-eslint/ban-ts-comment': 'off',
			'@typescript-eslint/ban-types': 'off',
			'@typescript-eslint/no-non-null-assertion': 'off',
			'@typescript-eslint/explicit-module-boundary-types': 'off',
			'@typescript-eslint/no-redeclare': 'off',
			'@typescript-eslint/no-non-null-asserted-optional-chain': 'off',
			'@typescript-eslint/no-unused-vars': 'warn',
			'vue/no-unused-vars': 'off',
			'vue/no-mutating-props': 'warn',
			'vue/custom-event-name-casing': 'off',
			'vue/attributes-order': 'off',
			'vue/one-component-per-file': 'off',
			'vue/html-closing-bracket-newline': 'off',
			'vue/max-attributes-per-line': 'off',
			'vue/multiline-html-element-content-newline': 'off',
			'vue/singleline-html-element-content-newline': 'off',
			'vue/attribute-hyphenation': 'off',
			'vue/valid-v-else': 'warn',
			'vue/no-deprecated-filter': 'warn',
			'vue/html-self-closing': 'off',
			'vue/no-multiple-template-root': 'off',
			'vue/require-default-prop': 'off',
			'vue/no-v-model-argument': 'off',
			'vue/no-arrow-functions-in-watch': 'off',
			'vue/no-template-key': 'off',
			'vue/no-v-for-template-key': 'warn',
			'vue/no-v-html': 'off',
			'vue/comment-directive': 'off',
			'vue/no-parsing-error': 'off',
			'vue/no-deprecated-v-on-native-modifier': 'off',
			'vue/multi-word-component-names': 'off',
			'no-constant-binary-expression': 'warn',
			'no-useless-escape': 'off',
			'no-sparse-arrays': 'off',
			'no-prototype-builtins': 'off',
			'no-constant-condition': 'off',
			'no-use-before-define': 'off',
			'no-restricted-globals': 'off',
			'no-restricted-syntax': 'off',
			'generator-star-spacing': 'off',
			'no-unreachable': 'off',
			'no-multiple-template-root': 'off',
			'no-unused-vars': 'off',
			'no-v-model-argument': 'off',
			'no-case-declarations': 'off',
			'no-console': 'off',
			'no-redeclare': 'off',
		},
	},
	{
		files: ['**/*.ts', '**/*.tsx', '**/*.vue'],

		rules: {
			'no-undef': 'off',
		},
	},
];
