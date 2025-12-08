# 启用UTF-8编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 获取脚本所在目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 设置默认参数
$moduleName = "apiServices"
$apiServicesPath = Join-Path (Join-Path $scriptDir "..") "src\api-services\"
$apiUrl = "http://localhost:5005/swagger/Default/swagger.json"

# 根据传入的参数修改配置
if ($args[0] -eq "approvalFlow") {
    $moduleName = "approvalFlow"
    $apiServicesPath = Join-Path (Join-Path $scriptDir "..") "src\api-plugins\approvalFlow\"
    $apiUrl = "http://localhost:5005/swagger/ApprovalFlow/swagger.json"
} elseif ($args[0] -eq "dingTalk") {
    $moduleName = "dingTalk"
    $apiServicesPath = Join-Path (Join-Path $scriptDir "..") "src\api-plugins\dingTalk\"
    $apiUrl = "http://localhost:5005/swagger/DingTalk/swagger.json"
} elseif ($args[0] -eq "goView") {
    $moduleName = "goView"
    $apiServicesPath = Join-Path (Join-Path $scriptDir "..") "src\api-plugins\goView\"
    # 注意：PowerShell会自动处理URL编码，所以不需要手动添加%20等
    $apiUrl = "http://localhost:5005/swagger/GoView/swagger.json"
}

# 输出信息和删除现有目录
Write-Output "================================ 删除目录 $moduleName ================================"
if (Test-Path $apiServicesPath) {
    Remove-Item -Path $apiServicesPath -Recurse -Force
}

# 开始生成代码
Write-Output "================================ 开始生成 $moduleName ================================"

# 输出调试信息
Write-Output "脚本目录: $scriptDir"
Write-Output "模块名称: $moduleName"
Write-Output "输出路径: $apiServicesPath"
Write-Output "API URL: $apiUrl"

# 执行代码生成命令
try {
    $codeGenJar = Join-Path $scriptDir "swagger-codegen-cli.jar"
    java -jar $codeGenJar generate -i $apiUrl -l typescript-axios -o $apiServicesPath
    if ($LASTEXITCODE -ne 0) {
        throw "Java命令执行失败，退出代码: $LASTEXITCODE"
    }
} catch {
    Write-Error "代码生成失败: $_"
    exit 1
}

# 删除不必要的文件和文件夹
Write-Output "================================ 删除不必要的文件和文件夹 ================================"

$filesToDelete = @(
    "$apiServicesPath\.swagger-codegen",
    "$apiServicesPath\.gitignore",
    "$apiServicesPath\.npmignore",
    "$apiServicesPath\.swagger-codegen-ignore",
    "$apiServicesPath\git_push.sh",
    "$apiServicesPath\package.json",
    "$apiServicesPath\README.md",
    "$apiServicesPath\tsconfig.json"
)

foreach ($filePath in $filesToDelete) {
    if (Test-Path $filePath) {
        if ((Get-Item $filePath) -is [System.IO.DirectoryInfo]) {
            Remove-Item -Path $filePath -Recurse -Force
        } else {
            Remove-Item -Path $filePath -Force
        }
    }
}

Write-Output "================================ 生成结束 ================================"