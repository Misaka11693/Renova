# `CardPro` 说明

`CardPro`卡片组件，基于`ElCard`二次封装，增加了大小、标题、图标等属性，可以直接写属性值实现，无需像官方版本一样通过`solt`实现。组件继承了官方版的所有属性、插槽，你也可以按官方版本一样的使用。

---

## 属性

| 属性名 | 说明 | 类型 | 默认值 |
| --- | --- | --- | ---  |
| title | 卡片的标题 | `string` | — |
| prefix-icon | 标题前方的图标 | `string` | — |
| suffix | header 右侧的内容，可以传入文本内容，也可以通过 `#footer` 传入 `solt`  | `string` | — |
| size | 尺寸, 可取值有 `large` 、`default` 、 `small` | `enum` | — |
| full-height | 是否 100% 高度，设置为 `true` 后，卡片将使用 flex 布局，父元素需有高度。卡片内容区将根据父元素高度自适应，无需再设置卡片 `style="height:100%" ` 和 `body-style="height:100%"` 。 | `boolean` | false |

## 如何使用

### 基本用法

卡片标题、内容以及图标，通过 `prefix-icon` 属性设置卡片图标。与官方版本不同的时，`CardPro` 通过 `title`
来设置标题，如果你使用 `header` 来设置，它将覆盖 `CardPro` 组件的特有属性，与官方版本完全一致。

```html
<template>
    <card-pro title="标题" prefix-icon="ele-DocumentCopy">
        <!-- 你的内容 -->
    </card-pro>
</template>
<script lang="ts" setup>
import CardPro from '/@/components/CardPro/index.vue';
</script>
```

### 后缀

通过 `suffix` 属性设置卡片右上角文本，你也可以通过 `solt` 来实现自定义内容

```html
<template>
    <card-pro title="标题" suffix="more >>>">
        <!-- 你的内容 -->
    </card-pro>
</template>
<script lang="ts" setup>
import CardPro from '/@/components/CardPro/index.vue';
</script>
```

使用  `solt` 方式来自定义后缀内容，你可以在其中添加按钮、链接、图标、输入框等

```html
<template>
    <card-pro title="标题">
        <template #suffix>
           <el-button type="primary" icon="ele-Refresh" round plain @click="reset">刷新</el-button>
        </template>

        <!-- 你的内容 -->
    </card-pro>
</template>
<script lang="ts" setup>
import CardPro from '/@/components/CardPro/index.vue';
</script>
```

### 不同尺寸

使用 `size` 属性配置尺寸，可使用 `large`、`default` 和 `small`

```html
<template>
    <card-pro title="标题" size="small">
        <!-- 你的内容 -->
    </card-pro>
</template>
<script lang="ts" setup>
import CardPro from '/@/components/CardPro/index.vue';
</script>
```

### 自适应100%高度

设置 `full-height` 属性为 `true` 即可

```html
<template>
    <card-pro title="标题" :full-height="true">
        <!-- 你的内容 -->
    </card-pro>
</template>
<script lang="ts" setup>
import CardPro from '/@/components/CardPro/index.vue';
</script>
```

---

最新更新于 2025.10.15
