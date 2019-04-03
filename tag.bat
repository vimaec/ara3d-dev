call :subroutine ..\ara3d-dev %1 %2
call :subroutine ..\math3d %1 %2
call :subroutine ..\revit-dev %1 %2
exit /b

rem <Directory> <version> <message>
:subroutine
pushd %1 
echo %2 > version.txt
git add version.txt 
git commit -m %3
git tag -a %2 -m %3 
git push --tags
popd
exit /b