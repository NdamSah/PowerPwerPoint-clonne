# Création de l'installeur
Comme vous pouvez le constater a la date ou je fais ma par du travail,le projet ne contient pas encore de fichier .csproj. Après création du projet WPF .NET 8 :
1. Compiler en Release : `dotnet publish -c Release -r win-x64 --self-contained true`
2. Récupérer le dossier `bin/Release/net8.0-windows/win-x64/publish`.
3. Générer l'installateur avec Inno Setup ou MSIX.
Merci de bien vouloir ne pas distribuer un installateur avant que la solution complète compile.