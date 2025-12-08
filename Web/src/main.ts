import '../lang/index'
import { createApp } from 'vue';
import pinia from '/@/stores/index';
import App from '/@/App.vue';
import router from '/@/router';
import { directive } from '/@/directive/index';
import other from '/@/utils/other';
import ElementPlus, { ElTooltip } from 'element-plus';
import '/@/theme/index.scss';
// 动画库
import 'animate.css';
// 栅格布局
import VueGridLayout from 'vue-grid-layout';
// 电子签名
import VueSignaturePad from 'vue-signature-pad';
// 组织架构图
import vue3TreeOrg from 'vue3-tree-org';
import 'vue3-tree-org/lib/vue3-tree-org.css';
// VForm3 表单设计
import VForm3 from 'vform3-builds';
import 'vform3-builds/dist/designer.style.css';
// 关闭自动打印
import { disAutoConnect } from 'vue-plugin-hiprint';
import sysDict from "/@/components/sysDict/sysDict.vue";
import multiLangInput from "/@/components/multiLangInput/index.vue";
disAutoConnect();

const app = createApp(App);

directive(app);
other.elSvg(app);

// 注册全局字典组件
app.component('GSysDict', sysDict);
// 注册全局多语言组件
app.component('GMultiLangInput', multiLangInput);

const TooltipProps = ElTooltip.props
TooltipProps.showAfter = { type: Number, default: 800 }; // 设置全局tooltip延时显示时间为800毫秒

app.use(pinia).use(router).use(ElementPlus).use(VueGridLayout).use(VForm3).use(VueSignaturePad).use(vue3TreeOrg).mount('#app');
