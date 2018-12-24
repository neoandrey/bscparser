C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /r:"lib\System.Data.SQLite.dll","lib\Newtonsoft.Json.dll","lib\System.Net.Http.dll"  /t:library  /out:lib\resources.dll src\ConnectionCipher.cs  src\ConnectionProperty.cs src\BscParserConfiguration.cs src\BscParserUtiLibrary.cs 

C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /r:"lib\System.Data.SQLite.dll","lib\Newtonsoft.Json.dll","lib\resources.dll" /out:bin\BscParser.exe src\BscParser.cs
pause
xcopy /e /f /y /d %cd%\lib\*.*  %cd%\bin\

if not exist .git\ (
   git init 
   git config  user.email  "neoandrey@yahoo.com"
   git config  user.name   "neoandrey@yahoo.com"
   echo "log" >>.gitignore
   echo "bin" >>.gitignore
   git add . 
   git commit -m "Initialize Application " 
)

