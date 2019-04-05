
call :subroutine ..\ara3d-dev %1 %2
call :subroutine ..\math3d %1 %2
call :subroutine ..\revit-dev %1 %2
exit /b

rem <Directory> <version> <message>
:subroutine
pushd %1 
echo %2 > version.txt
git add .
git commit -m %3
git push
git tag -a %2 -m %3 
git push --tags
popd
exit /b