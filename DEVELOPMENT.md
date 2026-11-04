# Dezvoltare

## Rulează din surse

Descarcă proiectul prin **Code > Download ZIP** sau clonează acest depozit. Pe Windows, `Ruleaza-jocul.bat` pregătește instrumentele oficiale necesare și pornește jocul. `Construieste-Windows.bat` produce EXE-ul în `builds/windows/`.

Pentru lucru în editor, instalează **Godot 4.7.2 .NET** și **.NET SDK 8 sau mai nou**, apoi importă [game/project.godot](game/project.godot). Ediția standard Godot fără .NET nu compilează codul C#.

```powershell
dotnet build game/BusinessSimulator.csproj
# Înlocuiește calea cu executabilul Godot .NET instalat.
& "C:/Tools/Godot/Godot_console.exe" --path game
```

Motorul, limbajul și datele au responsabilități separate: Godot afișează și primește comenzile, C# aplică regulile, iar JSON descrie conținutul.

## Unde modifici jocul

| Vrei să schimbi | Modul |
| --- | --- |
| Pornirea, navigarea și ciclul aplicației | [game/core/Main.cs](game/core/Main.cs), `Main.Navigation.cs` |
| Structura unei salvări și definițiile datelor | [game/contracts/](game/contracts/) |
| Produse, costuri, scenarii și evenimente | [game/data/](game/data/) |
| Tranzacții, prețuri și capacitate | [game/economy/SessionEconomy.cs](game/economy/SessionEconomy.cs) |
| Zile, lucrări și rezultatul campaniei | [game/gameplay/SessionGameplay.cs](game/gameplay/SessionGameplay.cs) |
| Evenimente și efecte temporare | [game/events/SessionEvents.cs](game/events/SessionEvents.cs) |
| Dependențele și efectele upgrade-urilor | [game/core/SessionDevelopment.cs](game/core/SessionDevelopment.cs) |
| Meniul principal, dificultăți și rezultate | [game/ui/screens/](game/ui/screens/) |
| Scena, producția, panourile și dialogurile | [game/ui/GameScreen.cs](game/ui/GameScreen.cs), [game/ui/game/](game/ui/game/) |
| Stiluri și controale comune | [game/ui/Ui.cs](game/ui/Ui.cs), `ThemeColors.cs` |
| Traduceri | [game/ui/Locale.cs](game/ui/Locale.cs), [Locale.Catalog.cs](game/ui/localization/Locale.Catalog.cs) |
| Diorama 3D | [game/core/BusinessWorld.cs](game/core/BusinessWorld.cs) |
| Salvare și verificarea actualizărilor | [SaveGame.cs](game/core/SaveGame.cs), [UpdateChecker.cs](game/core/UpdateChecker.cs) |
| Verificări automate | [game/tests/integration/](game/tests/integration/) |

`Session` este o singură clasă împărțită prin `partial` pe responsabilități. Interfața apelează operațiile sesiunii; acestea validează condițiile și modifică `GameState`. Evenimentul `Changed` cere actualizarea ecranului. Părțile `GameScreen.*` organizează același ecran în blocuri: layout, navigare, producție, panouri și dialoguri. Fiecare fișier începe cu o scurtă descriere a responsabilității sale.

Datele se încarcă din JSON; nu există bază de date sau server de joc. Timpul avansează doar prin Încheie ziua. Soldul reprezintă banii disponibili, iar profitul scade datoria rămasă și capitalul inițial, inclusiv grantul.

## Verificări și export

```powershell
# Compilare și verificarea regulilor jocului.
powershell -NoProfile -File tools/Ruleaza_Jocul.ps1 -SelfTest
# Parcurgerea interfeței, pentru ambele scenarii și toate dificultățile.
powershell -NoProfile -File tools/Ruleaza_Jocul.ps1 -QaTest
# EXE portabil, inclusiv verificarea pachetului.
powershell -NoProfile -File tools/Construieste_Windows.ps1
```

Testele de interfață injectează comenzile în Godot și verifică efectul lor asupra partidei. `--balance-audit` și `--balance-audit-holdout` compară strategii automate pe secvențe diferite de evenimente; rezultatele lor nu reprezintă rata de succes a tuturor jucătorilor.

Pentru Android sunt necesare .NET SDK 9 sau mai nou, JDK 17, Android SDK și șabloanele de export Godot 4.7.2 .NET. Setează `JAVA_HOME` și `ANDROID_HOME`, apoi variabilele `GODOT_ANDROID_KEYSTORE_RELEASE_PATH`, `GODOT_ANDROID_KEYSTORE_RELEASE_USER` și `GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD`. Rulează `tools/Construieste_Android.ps1`. APK-ul rezultat este în `builds/android/`.

Cheile și parolele se păstrează în afara depozitului. Folosește aceeași cheie pentru actualizări. Pachetele publicate se descarcă din [Releases](https://github.com/Chuvachox/FirmoQuest/releases); directorul `builds/` și cache-urile nu sunt versionate.

Pentru o schimbare, creează o ramură, modifică modulul relevant și rulează verificările afectate. Păstrează compatibilitatea salvărilor și identificatorii existenți din date. Trimite un pull request cu rezultatul și verificările efectuate.

## Autori și resurse

Codul este disponibil sub [licența MIT](LICENSE).

Demo inițial de gameplay și interfață: Victor. Coordonare, integrare și distribuire: Danila. Autorii, proveniența resurselor și condițiile componentelor externe sunt descrise în [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
