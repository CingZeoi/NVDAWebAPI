# NVDA Web API 语音控制器

一个轻量级的、独立的 Web 服务器，通过一套简洁强大的 RESTful API，封装了 [NVDA 屏幕阅读器](https://www.nvaccess.org/) 的语音功能。

本项目旨在充当您的应用程序与 NVDA 辅助功能之间的桥梁，允许开发者通过任何支持标准 HTTP 请求的编程语言、自动化脚本或工具，以编程方式控制 NVDA 的语音和盲文输出。

## 主要功能

*   **检查状态**: 验证 NVDA 是否正在运行，并获取其进程 ID。
*   **文本转语音**: 发送文本，让 NVDA 进行朗读。
*   **取消朗读**: 立刻中断 NVDA 当前的语音输出。
*   **盲文显示**: 发送文本，使其在连接的盲文设备上显示。
*   **端口可配置**: 使用命令行参数 `-p` 在任意端口上运行服务。
*   **标准化 JSON 响应**: 为所有请求提供可预测且易于解析的 JSON 响应。
*   **内置 JS 客户端**: 提供一个自托管的 JavaScript 库，将所有 API 封装为简单的异步函数，极大地方便了前端网页集成。

## 系统要求

-   **操作系统**: Windows
-   **NVDA**: 必须已安装并在运行状态，API 才能正常工作。
-   **.NET Framework**: 本程序需要 .NET Framework 4.8。

### 快速开始

要启动 API 服务器，只需在命令提示符 (CMD) 或 PowerShell 中运行可执行文件。

#### 使用默认端口

默认情况下，服务器会监听 `12320` 端口。

```shell
nvdawebapi.exe
```

现在，API 服务已在 `http://localhost:12320` 上可用。

#### 使用自定义端口

您可以使用 `-p` 参数指定一个自定义端口，这在默认端口被占用时非常有用。

例如，在 `58080` 端口上运行服务器：

```shell
nvdawebapi.exe -p 58080
```
API 服务将在 `http://localhost:58080` 上可用。

## API 文档

### 基础 URL

所有 API 端点的路径都以此为前缀：
`/api/nvda/`

### 响应体结构

所有请求，无论成功与否，都会返回一个结构统一的 JSON 对象。

```json
{
  "status": 0,
  "message": "Success",
  "result": { ... }
}```
```

-   **`status`** `(整数)`: 表示 API 请求本身的执行状态。
    -   `0`: 表示服务器成功处理了该 HTTP 请求。
    -   `> 0` (例如 `10`): 表示服务器端发生了异常 (如 `nvdaControllerClient.dll` 无法加载)。此时 `result` 字段将为 `null`。

-   **`message`** `(字符串)`: 对 `status` 状态码的文字描述。

-   **`result`** `(对象 | null)`: 包含与 NVDA 交互的结果。
    -   当 `status` 为 `0` 时，此对象将包含具体结果。
        -   **`code`** `(整数)`: NVDA 控制器函数的返回码。`0` 表示成功，非零值表示失败 (例如，NVDA 未运行)。
        -   **`description`** `(字符串)`: 对 NVDA 返回码 `code` 的描述信息。
    -   当 `status` 大于 `0` 时，`result` 字段为 `null`。

---

### API 调用端点详解

#### 1. 检查 NVDA 状态

获取 NVDA 的运行状态及其进程 ID。

-   **方法**: `GET`
-   **地址**: `/check_running`

**请求体**: 无

**成功响应 (`status: 0`)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "isRunning": true,
        "processId": 12468
    }
}
```

**NVDA 未运行时的响应 (`status: 0`)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "isRunning": false,
        "processId": 0
    }
}
```

**cURL 调用示例**
```bash
curl -X GET "http://localhost:12320/api/nvda/check_running"
```

---

#### 2. 朗读文本

提交一段文本，让 NVDA 进行朗读。

-   **方法**: `POST`
-   **地址**: `/speak_text`

**请求体 (JSON)**
```json
{
    "text": "你好，世界"
}
```

**成功响应 (`status: 0, result.code: 0`)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "code": 0,
        "description": "Request submitted"
    }
}
```

**失败响应 (NVDA 未运行)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "code": 1722,
        "description": "NVDA is not running"
    }
}
```

**cURL 调用示例**
```bash
curl -X POST "http://localhost:12320/api/nvda/speak_text" \
-H "Content-Type: application/json" \
-d "{\"text\":\"你好，世界\"}"
```

---

#### 3. 取消朗读

立即终止 NVDA 正在进行的所有朗读任务。

-   **方法**: `POST`
-   **地址**: `/cancel_speech`

**请求体**: 无

**成功响应 (`status: 0, result.code: 0`)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "code": 0,
        "description": "Request submitted"
    }
}
```

**cURL 调用示例**
```bash
curl -X POST "http://localhost:12320/api/nvda/cancel_speech"
```

---

#### 4. 显示盲文消息

在已连接的盲文点显器上显示一条消息。

-   **方法**: `POST`
-   **地址**: `/show_braille-message`

**请求体 (JSON)**
```json
{
    "text": "你好"
}
```

**成功响应 (`status: 0, result.code: 0`)**
```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "code": 0,
        "description": "Request submitted"
    }
}
```

**cURL 调用示例**
```bash
curl -X POST "http://localhost:12320/api/nvda/show_braille-message" \
-H "Content-Type: application/json" \
-d "{\"text\":\"你好\"}"
```

---

## JavaScript 客户端库

本Nvda Web API项目还内置一个 JS 客户端库，用于在网页中方便地调用 API。

### 1. 引入库

在 HTML 中添加以下 `<script>` 标签。库会自动发现服务器地址和端口。

```html
<!-- 确保端口号与您的服务启动端口一致 -->
<script src="http://localhost:12320/lib/nvda_api.js" defer></script>
```

### 2. API 调用

引入后，可通过全局对象 `NVDA_API` 调用以下异步函数：

#### `NVDA_API.checkRunning()`

检查 NVDA 是否运行。

*   **返回**: `Promise<number>` - 成功则返回 NVDA 进程 ID (PID)，失败返回 `0`。
*   **示例**: `const pid = await NVDA_API.checkRunning();`

---

#### `NVDA_API.speakText(text)`

请求 NVDA 朗读文本。

*   **参数**: `text` (string) - 要朗读的文本。
*   **返回**: `Promise<boolean>` - 成功发送返回 `true`，否则返回 `false`。
*   **示例**: `const success = await NVDA_API.speakText("你好");`

---

#### `NVDA_API.cancelSpeech()`

中断当前朗读。

*   **返回**: `Promise<boolean>` - 成功发送请求返回 `true`，否则返回 `false`。
*   **示例**: `await NVDA_API.cancelSpeech();`

---

#### `NVDA_API.showBrailleMessage(text)`

在盲文显示器上显示文本。

*   **参数**: `text` (string) - 要显示的文本。
*   **返回**: `Promise<boolean>` - 成功发送返回 `true`，否则返回 `false`。
*   **示例**: `await NVDA_API.showBrailleMessage("OK");`

## 参考资料

本项目的核心功能是基于 NVDA 官方提供的 `nvdaControllerClient` 接口实现的。所有与 NVDA 的底层交互均通过调用 `nvdaControllerClient.dll` 完成。

*   [**NVDA Controller Client Official Documentation**](https://github.com/nvaccess/nvda/blob/master/extras/controllerClient/readme.md)
