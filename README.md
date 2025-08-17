# NVDA Web API

A lightweight, standalone web server that wraps the core features of the [NVDA screen reader](https://www.nvaccess.org/) into a clean and simple RESTful API.

This project acts as a bridge between your applications and NVDA's accessibility features, allowing you to programmatically control NVDA's speech and braille output from any programming language, automation script, or tool that can make standard HTTP requests.

## Features

*   **Check Status**: Verify if NVDA is running and get its process ID.
*   **Text-to-Speech**: Send text to be spoken aloud by NVDA.
*   **Cancel Speech**: Instantly stop NVDA's current speech.
*   **Braille Display**: Send text to be displayed on a connected braille device.
*   **Configurable Port**: Run the server on any port using the `-p` command-line flag.
*   **Standardized JSON Responses**: Clean, predictable JSON responses for every request.
*   **Built-in JS Client**: Comes with a self-hosted JavaScript library that wraps all API calls into simple async functions, making web integration a breeze.

## Requirements

*   **OS**: Windows
*   **NVDA**: Must be installed and running for the API to work.
*   **.NET Framework**: .NET Framework 4.8 is required.

## Quick Start

To fire up the API server, just run the executable from Command Prompt (CMD) or PowerShell.

#### Default Port

By default, the server listens on port `12320`.

```shell
nvdawebapi.exe
```
The API is now available at `http://localhost:12320`.

#### Custom Port

Use the `-p` flag to specify a custom port, which is handy if the default is already in use.

For example, to run on port `58080`:

```shell
nvdawebapi.exe -p 58080
```
The API will be available at `http://localhost:58080`.

## API Reference

### Base URL

All API endpoints are prefixed with:
`/api/nvda/`

### Response Body Structure

All requests, successful or not, return a consistent JSON object.

```json
{
  "status": 0,
  "message": "Success",
  "result": { ... }
}
```

*   **`status`** `(integer)`: The execution status of the API request itself.
    *   `0`: The server handled the HTTP request successfully.
    *   `> 0` (e.g., `10`): A server-side exception occurred (like `nvdaControllerClient.dll` failed to load). The `result` field will be `null`.

*   **`message`** `(string)`: A human-readable description of the `status` code.

*   **`result`** `(object | null)`: Contains the result of the interaction with NVDA.
    *   When `status` is `0`, this object contains the details.
        *   **`code`** `(integer)`: The return code from the NVDA controller function. `0` means success, while a non-zero value indicates a failure (e.g., NVDA is not running).
        *   **`description`** `(string)`: A description of the NVDA return `code`.
    *   When `status` is `> 0`, `result` will be `null`.

---

### API Endpoints

#### 1. Check NVDA Status

Gets NVDA's running status and its process ID.

*   **Method**: `GET`
*   **Endpoint**: `/check_running`

**Payload**: None

**Success Response (`status: 0`)**
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

**Response when NVDA is not running (`status: 0`)**
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

**cURL Example**
```bash
curl -X GET "http://localhost:12320/api/nvda/check_running"
```

---

#### 2. Speak Text

Submits text for NVDA to speak.

*   **Method**: `POST`
*   **Endpoint**: `/speak_text`

**JSON Payload**
```json
{
    "text": "Hello, World"
}
```

**Success Response (`status: 0, result.code: 0`)**
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

**Failure Response (NVDA not running)**
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

**cURL Example**
```bash
curl -X POST "http://localhost:12320/api/nvda/speak_text" \
-H "Content-Type: application/json" \
-d "{\"text\":\"Hello, World\"}"
```

---

#### 3. Cancel Speech

Instantly stops any speech currently being output by NVDA.

*   **Method**: `POST`
*   **Endpoint**: `/cancel_speech`

**Payload**: None

**Success Response (`status: 0, result.code: 0`)**```json
{
    "status": 0,
    "message": "Success",
    "result": {
        "code": 0,
        "description": "Request submitted"
    }
}
```

**cURL Example**
```bash
curl -X POST "http://localhost:12320/api/nvda/cancel_speech"
```

---

#### 4. Show Braille Message

Displays a message on a connected braille display.

*   **Method**: `POST`
*   **Endpoint**: `/show_braille-message`

**JSON Payload**
```json
{
    "text": "Hello"
}
```

**Success Response (`status: 0, result.code: 0`)**
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

**cURL Example**
```bash
curl -X POST "http://localhost:12320/api/nvda/show_braille-message" \
-H "Content-Type: application/json" \
-d "{\"text\":\"Hello\"}"
```

---

## JavaScript Client Library

This project includes a built-in JS client library to make calling the API from a web page super easy.

### 1. Include the Library

Add the following `<script>` tag to your HTML. The library automatically discovers the server's address and port.

```html
<!-- Make sure the port matches the one your server is running on -->
<script src="http://localhost:12320/lib/nvda_api.js" defer></script>```

### 2. Calling the API

Once included, a global `NVDA_API` object is available. You can call the following async functions:

#### `NVDA_API.checkRunning()`

Checks if NVDA is running.

*   **Returns**: `Promise<number>` - The NVDA process ID (PID) on success, or `0` on failure.
*   **Example**: `const pid = await NVDA_API.checkRunning();`

---

#### `NVDA_API.speakText(text)`

Tells NVDA to speak some text.

*   **Params**: `text` (string) - The text to speak.
*   **Returns**: `Promise<boolean>` - `true` if the request was sent successfully, otherwise `false`.
*   **Example**: `const success = await NVDA_API.speakText("Hello");`

---

#### `NVDA_API.cancelSpeech()`

Stops the current speech.

*   **Returns**: `Promise<boolean>` - `true` if the request was sent successfully, otherwise `false`.
*   **Example**: `await NVDA_API.cancelSpeech();`

---

#### `NVDA_API.showBrailleMessage(text)`

Displays text on a braille display.

*   **Params**: `text` (string) - The text to display.
*   **Returns**: `Promise<boolean>` - `true` if the request was sent successfully, otherwise `false`.
*   **Example**: `await NVDA_API.showBrailleMessage("OK");`

---

## Acknowledgements

The core functionality of this project is made possible by the official NVDA `nvdaControllerClient` interface. All low-level interaction with NVDA is handled by `nvdaControllerClient.dll`.

*   [**NVDA Controller Client Official Documentation**](https://github.com/nvaccess/nvda/blob/master/extras/controllerClient/readme.md)

---

## Author Introduction

**CIRONG ZHANG**, Accessibility Advocate.

With years of dedication to researching and implementing accessibility solutions across web, PC, and mobile platforms, he possesses distinctive theoretical frameworks and extensive hands-on expertise in cross-platform inclusive design.
