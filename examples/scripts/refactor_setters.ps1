$root = Get-Location
$pattern = 'new\s+([A-Za-z_][A-Za-z0-9_<>]*?)\s*\(\s*\)'

function Scan-Args {
    param($text, $start)
    $depth = 0
    $i = $start
    $in_string = $null
    $esc = $false
    $n = $text.Length
    while ($i -lt $n) {
        $ch = $text[$i]
        if ($in_string) {
            if ($esc) { $esc = $false }
            elseif ($ch -eq '\') { $esc = $true }
            elseif ($ch -eq $in_string) { $in_string = $null }
        } else {
            if ($ch -eq '"' -or $ch -eq "'") { $in_string = $ch }
            elseif ($ch -eq '(') { $depth++ }
            elseif ($ch -eq ')') { $depth--; if ($depth -eq 0) { return $i } }
        }
        $i++
    }
    return -1
}

$changed_files = @()
Get-ChildItem -Path $root -Recurse -Filter *.cs | ForEach-Object {
    $path = $_.FullName
    $text = Get-Content $path -Raw
    $out = New-Object System.Collections.Generic.List[string]
    $idx = 0
    $modified = $false
    while ($true) {
        $m = [regex]::Match($text, $pattern, [System.Text.RegularExpressions.RegexOptions]::None, [timespan]::FromSeconds(10))
        if (-not $m.Success) {
            $out.Add($text.Substring($idx))
            break
        }
        $start = $m.Index
        $end = $m.Index + $m.Length
        $out.Add($text.Substring($idx, $start - $idx))
        $cur = $end
        $set_props = New-Object System.Collections.Generic.List[Tuple[string,string]]
        while ($true) {
            while ($cur -lt $text.Length -and [char]::IsWhiteSpace($text[$cur])) { $cur++ }
            if ($cur -ge $text.Length -or $text[$cur] -ne '.') { break }
            $meth_start = $cur + 1
            $meth_end = $meth_start
            while ($meth_end -lt $text.Length -and ([char]::IsLetterOrDigit($text[$meth_end]) -or $text[$meth_end] -eq '_')) { $meth_end++ }
            $meth = $text.Substring($meth_start, $meth_end - $meth_start)
            if (-not $meth.StartsWith('Set') -or $meth_end -ge $text.Length -or $text[$meth_end] -ne '(') { break }
            $arg_start = $meth_end
            $arg_end = Scan-Args $text $arg_start
            if ($arg_end -eq -1) { break }
            $arg_text = $text.Substring($arg_start + 1, $arg_end - $arg_start - 1).Trim()
            $prop_name = $meth.Substring(3)
            if ($prop_name) {
                $set_props.Add([Tuple]::Create($prop_name, $arg_text))
                $cur = $arg_end + 1
                continue
            }
            break
        }
        if ($set_props.Count -gt 0) {
            $line_start = $text.LastIndexOf("`n", $start) + 1
            $prefix = $text.Substring($line_start, $start - $line_start)
            $indent = $prefix -replace "`t", " "
            if ($set_props.Count -eq 1) {
                $rep = "new $($m.Groups[1].Value) { $($set_props[0].Item1) = $($set_props[0].Item2) }"
            } else {
                $rep_lines = @("new $($m.Groups[1].Value)", "{")
                foreach ($prop in $set_props) {
                    $rep_lines += "    $($prop.Item1) = $($prop.Item2),"
                }
                $rep_lines += "}"
                $rep = $rep_lines -join "`n"
                $rep = $rep -replace "`n", "`n$indent"
            }
            $out.Add($rep)
            $idx = $cur
            $modified = $true
        } else {
            $out.Add($text.Substring($start, $end))
            $idx = $end
        }
    }
    if ($modified) {
        $new_text = $out -join ""
        Set-Content $path $new_text -Encoding UTF8
        $changed_files += $path
    }
}

Write-Host "Processed $($changed_files.Count) files"
foreach ($path in $changed_files) {
    Write-Host $path
}