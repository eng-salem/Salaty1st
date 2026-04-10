@echo off  
msiexec /x {31E73EE1-C403-4424-908E-BCA7C3E47786} /qn /norestart  
msiexec /i \"%~dp0Salaty.setup\Release\Salaty.setup.msi\" /passive  
pause 
