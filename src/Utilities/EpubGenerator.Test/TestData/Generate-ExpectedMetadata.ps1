# EPUB 集成测试 - 元数据生成脚本
# 用法: .\Generate-ExpectedMetadata.ps1
# 此脚本会扫描 Input 文件夹中的所有 txt 文件，解析章节，并生成预期的元数据 JSON

param(
    [string]$InputDir = "$PSScriptRoot\Input",
    [string]$OutputDir = "$PSScriptRoot\Expected"
)

# 设置 UTF-8 编码
$null = chcp 65001
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# 注册 GB2312/GBK 编码提供程序
[System.Text.Encoding]::RegisterProvider([System.Text.CodePagesEncodingProvider]::Instance)

# 检测文件编码的函数
function Get-FileEncoding {
    param([string]$FilePath)
    
    # 只读取前 4KB 来检测 BOM 和编码
    $stream = [System.IO.File]::OpenRead($FilePath)
    try {
        $buffer = [byte[]]::new([Math]::Min(4096, $stream.Length))
        $bytesRead = $stream.Read($buffer, 0, $buffer.Length)
        $bytes = $buffer[0..($bytesRead - 1)]
    } finally {
        $stream.Close()
    }
    
    # 检查 BOM
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        return [System.Text.Encoding]::UTF8
    }
    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        return [System.Text.Encoding]::Unicode
    }
    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        return [System.Text.Encoding]::BigEndianUnicode
    }
    
    # 尝试 UTF-8 无 BOM
    try {
        $utf8NoBom = [System.Text.UTF8Encoding]::new($false, $true)
        $null = $utf8NoBom.GetString($bytes)
        return [System.Text.Encoding]::UTF8
    } catch {
        # 不是有效的 UTF-8，尝试 GBK/GB2312
        return [System.Text.Encoding]::GetEncoding("gb2312")
    }
}

# 默认中文章节正则表达式
$DefaultChapterPattern = '第[零一二三四五六七八九十百千万\d]+[章节回卷集部篇]'

function Get-ChapterPattern {
    param([string]$FirstLine)
    
    # 检查第一行是否是自定义正则表达式（以 / 开头和结尾）
    if ($FirstLine -match '^/(.+)/$') {
        return @{
            Pattern = $Matches[1]
            IsCustom = $true
        }
    }
    
    return @{
        Pattern = $DefaultChapterPattern
        IsCustom = $false
    }
}

# 高性能章节分割函数 - 逐行处理，避免大量内存分配
# 注意：与 C# RegexTextSplitter 行为一致，第一个章节之前的内容会作为"前言"章节
# 内容长度计算：累加每行 trimmed 长度 + 换行符（与 C# StringBuilder.AppendLine + Trim 一致）
function Split-TextByChaptersOptimized {
    param(
        [string]$FilePath,
        [System.Text.Encoding]$Encoding,
        [string]$Pattern,
        [bool]$SkipFirstLine
    )
    
    $chapters = [System.Collections.Generic.List[hashtable]]::new()
    $regex = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Compiled)
    
    $currentChapterTitle = $null
    $currentChapterLines = [System.Collections.Generic.List[string]]::new()
    $chapterIndex = 0
    $lineNumber = 0
    $contentBeforeFirstChapterLines = [System.Collections.Generic.List[string]]::new()
    $foundFirstChapter = $false
    
    # 使用 StreamReader 逐行读取，内存效率高
    $reader = [System.IO.StreamReader]::new($FilePath, $Encoding)
    try {
        while ($null -ne ($line = $reader.ReadLine())) {
            $lineNumber++
            
            # 跳过第一行（如果是自定义正则）
            if ($SkipFirstLine -and $lineNumber -eq 1) {
                continue
            }
            
            $trimmedLine = $line.Trim()
            
            # 检查是否是章节标题
            if ($regex.IsMatch($trimmedLine)) {
                # 如果这是第一个章节，且之前有内容，先保存"前言"
                if (-not $foundFirstChapter -and $contentBeforeFirstChapterLines.Count -gt 0) {
                    # 计算内容长度：模拟 StringBuilder.AppendLine + Trim
                    $contentLength = ($contentBeforeFirstChapterLines -join "`n").Trim().Length
                    $chapters.Add(@{
                        Title = "前言"
                        ContentLength = $contentLength
                        Index = $chapterIndex
                    })
                    $chapterIndex++
                }
                $foundFirstChapter = $true
                
                # 保存上一章节
                if ($null -ne $currentChapterTitle) {
                    $contentLength = ($currentChapterLines -join "`n").Trim().Length
                    $chapters.Add(@{
                        Title = $currentChapterTitle
                        ContentLength = $contentLength
                        Index = $chapterIndex
                    })
                    $chapterIndex++
                }
                
                # 开始新章节
                $currentChapterTitle = $trimmedLine
                $currentChapterLines = [System.Collections.Generic.List[string]]::new()
            } else {
                # 累加内容（非空行或保留空行）
                if ($null -ne $currentChapterTitle) {
                    if ($trimmedLine.Length -gt 0) {
                        $currentChapterLines.Add($trimmedLine)
                    }
                } elseif (-not $foundFirstChapter -and $trimmedLine.Length -gt 0) {
                    # 第一个章节之前的内容
                    $contentBeforeFirstChapterLines.Add($trimmedLine)
                }
            }
            
            # 每 10000 行输出进度
            if ($lineNumber % 10000 -eq 0) {
                Write-Host "    已处理 $lineNumber 行..." -ForegroundColor DarkGray
            }
        }
        
        # 保存最后一个章节
        if ($null -ne $currentChapterTitle) {
            $contentLength = ($currentChapterLines -join "`n").Trim().Length
            $chapters.Add(@{
                Title = $currentChapterTitle
                ContentLength = $contentLength
                Index = $chapterIndex
            })
        }
        
        # 如果没有匹配到任何章节
        if ($chapters.Count -eq 0) {
            # 需要重新读取计算总长度，使用与有章节时相同的方式
            $reader.BaseStream.Position = 0
            $reader.DiscardBufferedData()
            $allLines = [System.Collections.Generic.List[string]]::new()
            $lineNumber = 0
            while ($null -ne ($line = $reader.ReadLine())) {
                $lineNumber++
                if ($SkipFirstLine -and $lineNumber -eq 1) { continue }
                $trimmedLine = $line.Trim()
                if ($trimmedLine.Length -gt 0) {
                    $allLines.Add($trimmedLine)
                }
            }
            $contentLength = ($allLines -join "`n").Trim().Length
            $chapters.Add(@{
                Title = "正文"
                ContentLength = $contentLength
                Index = 0
            })
        }
    } finally {
        $reader.Close()
    }
    
    return $chapters
}

# 确保输出目录存在
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# 检查输入目录是否存在
if (-not (Test-Path $InputDir)) {
    Write-Host "Input 目录不存在: $InputDir" -ForegroundColor Yellow
    Write-Host "请创建 Input 文件夹并添加测试用的 txt 文件" -ForegroundColor Yellow
    exit 0
}

# 获取所有 txt 文件
$txtFiles = Get-ChildItem -Path $InputDir -Filter "*.txt"

if ($txtFiles.Count -eq 0) {
    Write-Host "未在 $InputDir 找到任何 txt 文件" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "使用说明:" -ForegroundColor Cyan
    Write-Host "1. 将 txt 文件放入 Input 文件夹"
    Write-Host "2. txt 文件第一行如果是 /正则表达式/ 格式，则使用自定义正则"
    Write-Host "3. 否则使用默认正则: $DefaultChapterPattern"
    Write-Host "4. 文件名将作为书籍标题"
    Write-Host ""
    Write-Host "示例 txt 文件格式:" -ForegroundColor Cyan
    Write-Host "/^第\d+章.*$/" -ForegroundColor Gray
    Write-Host "第1章 开始" -ForegroundColor Gray
    Write-Host "这是第一章的内容..." -ForegroundColor Gray
    exit 0
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "EPUB 集成测试 - 元数据生成器" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

foreach ($file in $txtFiles) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    Write-Host "处理: $($file.Name) ($([Math]::Round($file.Length / 1MB, 2)) MB)" -ForegroundColor Green
    
    # 自动检测文件编码
    $encoding = Get-FileEncoding -FilePath $file.FullName
    Write-Host "  检测到编码: $($encoding.EncodingName)" -ForegroundColor Cyan
    
    # 只读取第一行来判断正则表达式
    $reader = [System.IO.StreamReader]::new($file.FullName, $encoding)
    try {
        $firstLine = $reader.ReadLine()
        if ($null -eq $firstLine) { $firstLine = "" }
        $firstLine = $firstLine.Trim()
    } finally {
        $reader.Close()
    }
    
    # 解析正则表达式
    $patternInfo = Get-ChapterPattern -FirstLine $firstLine
    $skipFirstLine = $patternInfo.IsCustom
    
    if ($patternInfo.IsCustom) {
        Write-Host "  使用自定义正则: $($patternInfo.Pattern)" -ForegroundColor Yellow
    } else {
        Write-Host "  使用默认正则: $($patternInfo.Pattern)" -ForegroundColor Gray
    }
    
    # 使用优化的章节分割函数
    $chapters = Split-TextByChaptersOptimized -FilePath $file.FullName -Encoding $encoding -Pattern $patternInfo.Pattern -SkipFirstLine $skipFirstLine
    
    Write-Host "  发现 $($chapters.Count) 个章节" -ForegroundColor Cyan
    
    # 只显示前 5 个和后 2 个章节
    $displayCount = [Math]::Min(5, $chapters.Count)
    for ($i = 0; $i -lt $displayCount; $i++) {
        $chapter = $chapters[$i]
        Write-Host "    [$($chapter.Index)] $($chapter.Title) ($($chapter.ContentLength) 字符)" -ForegroundColor Gray
    }
    if ($chapters.Count -gt 7) {
        Write-Host "    ... 省略 $($chapters.Count - 7) 个章节 ..." -ForegroundColor DarkGray
        for ($i = $chapters.Count - 2; $i -lt $chapters.Count; $i++) {
            $chapter = $chapters[$i]
            Write-Host "    [$($chapter.Index)] $($chapter.Title) ($($chapter.ContentLength) 字符)" -ForegroundColor Gray
        }
    } elseif ($chapters.Count -gt $displayCount) {
        for ($i = $displayCount; $i -lt $chapters.Count; $i++) {
            $chapter = $chapters[$i]
            Write-Host "    [$($chapter.Index)] $($chapter.Title) ($($chapter.ContentLength) 字符)" -ForegroundColor Gray
        }
    }
    
    # 生成元数据
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    # 确保 Chapters 始终是数组（即使只有一个元素）
    $chaptersArray = @($chapters | ForEach-Object {
        @{
            Index = $_.Index
            Title = $_.Title
            ContentLength = $_.ContentLength
        }
    })
    
    $metadata = @{
        SourceFile = $file.Name
        Title = $baseName
        Author = "测试作者"
        Language = "zh"
        ChapterPattern = $patternInfo.Pattern
        IsCustomPattern = $patternInfo.IsCustom
        TotalChapters = $chapters.Count
        Chapters = $chaptersArray
        GeneratedAt = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    }
    
    # 保存为 JSON（UTF-8 无 BOM）
    $jsonPath = Join-Path $OutputDir "$baseName.json"
    $jsonContent = $metadata | ConvertTo-Json -Depth 10
    [System.IO.File]::WriteAllText($jsonPath, $jsonContent, [System.Text.UTF8Encoding]::new($false))
    
    $stopwatch.Stop()
    Write-Host "  已生成: $baseName.json (耗时 $([Math]::Round($stopwatch.Elapsed.TotalSeconds, 2)) 秒)" -ForegroundColor Green
    Write-Host ""
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "完成！共处理 $($txtFiles.Count) 个文件" -ForegroundColor Cyan
Write-Host "元数据已保存到: $OutputDir" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
