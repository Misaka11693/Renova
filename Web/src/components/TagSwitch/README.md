# `TagSwitch` 使用说明

`TagSwitch`，是基于 [喵你个汪](https://https://gitee.com/jasondom) PR的 `SysDict`组件封装的标签开关，展示状态下显示为 tag 标签，需要修改时，移动鼠标至 `tag` 上将自动切换为 `switch` ，点击即可切换。本组件与 `switch` 一样，适用于仅有两个不同值的场景(如： 启用/禁用、 是/否、 男/女 等)。

---

## 如何使用

```html
<template>
	<tag-switch v-model="你要绑定的值" :active-value="switch打开时的值" :inactive-value="switch关闭时的值" code="字典编码" @change="change回调的方法" />
</template>
<script lang="ts" setup>
import TagSwitch from '/@/components/TagSwitch/index.vue';
</script>
```

注意：`code` 必须为在系统中已经配置的数据字典

---

最新更新于 2025.09.29
