# HardLink
Hardlink Version 1.0

Kopiert alle Dateien aus den "--source-folder" nach "--destination" und ver-
gleicht sie dabei mit den Dateien unter "--link-dest". Sind sie identisch, wird 
ein NTFS-Hardlink angelegt, ansonsten eine neue Kopie der Datei. Das ganze 
geschiet multithreaded.

Verwendung:

hardlink.exe --source-folder "C:\Quellverz1" "C:\Quellverz2" --destination "D:\Backup" --link-dest "D:\Backup\Verz1" --logfile "D:\Backup\Log.txt"

hardlink.exe -s "C:\Quellverz1" "C:\Quellverz2" -d "D:\Backup" -t "D:\Backup\Verz1"

Optionen:

-h --help           Zeigt diese Hilfe an.
-s --source-folder  Auflistung aller Quellverzeichnisse. Ein Leerzeichen dient 
                    als Trennzeichen zwischen mehreren Verzeichnissen. Kommt 
                    ein Leerzeichen im Pfad vor, so muss das Verzeichnis in An-
                    f¸hrungszeichen gesetzt werden.
-d --destination    Zielverzeichnis, unter denen eine Kopie der Quellverzeich-
                    nisse abgelegt wird.
-t --link-dest      Ziel der Hardlinks. Wenn die Dateien hier mit denen aus den
                    Quellverzeichnissen identisch sind, werden sie verlinkt, 
                    ansonsten wird eine neue Kopie angelegt.
-l --logfile        Logdatei mit vollst‰ndigem Pfad.

# HardLink Backup
Um basierend auf diesem Tool ein einfaches, aber robustes Datei-Backup durchzuführen, siehe https://github.com/linuzer/HardlinkBackup
