
del x-bkemugid\Img\ANDOS.IMG
@if exist "x-bkemugid\Img\ANDOS.IMG" (
  echo.
  echo ####### FAILED to delete old disk image file #######
  exit /b
)

copy "x-bkemugid\Img\ANDOS_.IMG " "x-bkemugid\Img\ANDOS.IMG"
bkdecmd a x-bkemugid/Img/ANDOS.IMG SABOT2.BIN
bkdecmd a x-bkemugid/Img/ANDOS.IMG S2SCRN.LZS
bkdecmd a x-bkemugid/Img/ANDOS.IMG S2CORE.LZS

start x-bkemugid\BK_x64.exe /C BK-0011M_FDD
