@echo OFF
 :begin
 REM ɾ��ǰ���ļ����ļ���
 DEL /f /s /q ".\Web\node_modules\*.*"
 RD /s /q ".\Web\node_modules"
 REM ѭ��ɾ��ָ���ļ����µ��ļ���
 FOR /d /r ".\Admin.NET\" %%b in (bin,obj,public) do rd /s /q "%%b"
 ECHO ��������ϣ���������˳���
 PAUSE>NUL
 EXIT
 GOTO BEGIN