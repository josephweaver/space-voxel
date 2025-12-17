@echo off

REM ----------------------------
REM Simple Git check-in script
REM Usage:
REM   checkin.bat "commit message"
REM ----------------------------

IF "%~1"=="" (
    echo ERROR: Commit message required.
    echo Usage: checkin.bat "your message here"
    exit /b 1
)

git status

git add -A
IF ERRORLEVEL 1 (
    echo ERROR: git add failed.
    exit /b 1
)

git commit -m "%~1"
IF ERRORLEVEL 1 (
    echo ERROR: git commit failed.
    exit /b 1
)

git push
IF ERRORLEVEL 1 (
    echo ERROR: git push failed.
    exit /b 1
)

echo.
echo Check-in complete.
