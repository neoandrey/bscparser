C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /r:"lib\System.Data.SQLite.dll","lib\Newtonsoft.Json.dll" /out:bin\BscParser.exe src\BscParser.cs
pause
xcopy /e /f /y %cd%\lib\report_resources.dll %cd%\bin\

if not exist .git\ (
   git init 
   git config  user.email  "neoandrey@yahoo.com"
   git config  user.name   "neoandrey@yahoo.com"
   echo "log" >>.gitignore
   echo "bin" >>.gitignore
   git add . 
   git commit -m "Initialize Application " 
)

