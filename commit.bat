call :subroutine ..\ara3d-dev %1
call :subroutine ..\math3d %1
call :subroutine ..\revit-dev %1
exit /b

rem <Directory> <message>
:subroutine
pushd %1 
git add .
git commit -m %2
rem git push
popd
exit /b