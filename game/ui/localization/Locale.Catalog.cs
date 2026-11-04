using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Catalogul textelor romanesti si al traducerilor ruse si engleze.
public static partial class Locale
{
    static Locale()
    {
        Add("CONSTRUIEȘTE CEVA AL TĂU", "СОЗДАЙ ЧТО-ТО СВОЁ", "BUILD SOMETHING OF YOUR OWN");
        Add("De la prima idee\nla o afacere în creștere.", "От первой идеи\nдо растущего бизнеса.", "From the first idea\nto a growing business.");
        Add("Planifică. Produce. Dezvoltă. Fiecare zi este o decizie.", "Планируй. Производи. Развивай. Каждый день - решение.", "Plan. Produce. Grow. Every day is a decision.");
        Add("Ferma ta sau studioul tău IT. O singură afacere, o mulțime de alegeri.", "Твоя ферма или твоя IT-студия. Один бизнес, множество решений.", "Your farm or your IT studio. One business, many choices.");
        Add("Continuă partida", "Продолжить игру", "Continue game");
        Add("O partidă nouă înlocuiește salvarea curentă.", "Новая игра заменит текущее сохранение.", "A new game replaces the current save.");
        Add("Joacă", "Играть", "Play");
        Add("Cum se joacă?", "Как играть?", "How to play?");
        Add("Ieșire", "Выход", "Exit");
        Add("{0} de zile · 2 scenarii · 3 dificultăți", "{0} дней · 2 сценария · 3 сложности", "{0} days · 2 scenarios · 3 difficulties");
        Add("Schimbă limba jocului", "Сменить язык игры", "Change the game language");
        Add("Limba jocului", "Язык игры", "Game language");
        Add("Închide", "Закрыть", "Close");
        Add("Beta 0.5 · Salvare locală automată", "Beta 0.5 · Локальное автосохранение", "Beta 0.5 · Automatic local save");
        Add("Modele 3D: Kenney · CC0", "3D-модели: Kenney · CC0", "3D models: Kenney · CC0");
        Add("Atinge un obiect pentru productie. Meniurile sunt in bara de jos.",
            "Нажми на объект для производства. Меню находятся на нижней панели.",
            "Tap an object to start production. Use the bottom bar for menus.");
        Add("Glisează în sus sau în jos pentru a derula. Înapoi: închide.",
            "Проведите вверх или вниз для прокрутки. Назад: закрыть.",
            "Swipe up or down to scroll. Back: close.");
        Add("Compara efectul inainte de cumparare. Viteza se aplica lucrarilor noi; recolta, preturile si costurile se recalculeaza automat.",
            "Сравни эффект до покупки. Скорость меняется для новых работ; урожай, цены и расходы пересчитываются сразу.",
            "Compare the effect before buying. Speed applies to new jobs; yield, prices and costs update immediately.");
        Add("{0} loturi / parcela", "{0} партий на участок", "{0} batches per plot");
        Add("Loturi / loc", "Партий на место", "Batches per slot");
        Add("Cost fix / zi", "Постоянные расходы в день", "Fixed cost per day");
        Add("unit.", "ед.", "units");
        Add("sac", "мешок", "bag");
        Add("set", "набор", "set");
        Add("buc", "шт.", "unit");
        Add("+1 unitate la fiecare recoltă. Permite 2 loturi pe parcela.",
            "+1 единица к каждому урожаю. До 2 партий на участке.",
            "+1 unit per harvest. Allows 2 batches per plot.");
        Add("+1 parcelă de cultură (producție simultană). Permite 3 loturi pe parcela.",
            "+1 участок для параллельного производства. До 3 партий на участке.",
            "+1 plot for simultaneous production. Allows 3 batches per plot.");
        Add("Obține un profit de cel puțin ținta zilnică în 30 de zile.",
            "Достигни целевой прибыли за 30 дней.", "Reach the profit target within 30 days.");
        Add("Simulator economic inspirat din realitatea Republicii Moldova",
            "Экономический симулятор на основе реалий Республики Молдова",
            "Economic simulator inspired by life in the Republic of Moldova");
        Add("Sămânță de grâu de toamnă.", "Семена озимой пшеницы.", "Winter wheat seeds.");
        Add("Sămânță hibrid de porumb.", "Гибридные семена кукурузы.", "Hybrid corn seeds.");
        Add("Răsaduri de roșii și castraveți.", "Рассада помидоров и огурцов.", "Tomato and cucumber seedlings.");
        Add("Recoltă de grâu vândută la siloz.", "Урожай пшеницы для продажи на элеватор.", "Wheat harvest sold to a grain silo.");
        Add("Porumb furajer, cerere constantă.", "Кормовая кукуруза со стабильным спросом.", "Feed corn with steady demand.");
        Add("Legume de sezon pentru piața locală.", "Сезонные овощи для местного рынка.", "Seasonal vegetables for the local market.");
        Add("Licențe, domenii și echipament pentru un proiect.", "Лицензии, домены и оборудование для проекта.", "Licenses, domains and equipment for a project.");
        Add("Resurse cloud pentru aplicații mobile.", "Облачные ресурсы для мобильных приложений.", "Cloud resources for mobile apps.");
        Add("Contract mic, se plătește la livrare.", "Небольшой контракт с оплатой при сдаче.", "Small contract paid on delivery.");
        Add("Abonament recurent, venit sigur.", "Регулярное обслуживание со стабильным доходом.", "Recurring maintenance with steady income.");
        Add("Contract mare, durează mai mult.", "Крупный контракт, требующий больше времени.", "Large contract that takes longer.");
        Add("Salvarea nu a reușit. Verifică spațiul și accesul la folderul jocului.",
            "Не удалось сохранить. Проверь свободное место и доступ к папке игры.",
            "Saving failed. Check free space and access to the game folder.");
        Add("Salvarea nu poate fi citită. Fișierul a fost păstrat.",
            "Не удалось прочитать сохранение. Файл сохранен.", "The save could not be read. The file was preserved.");

        Add("Primul tău ciclu de afaceri", "Твой первый цикл бизнеса", "Your first business cycle");
        Add("Un pas pe rând. Jocul așteaptă decizia ta.", "Шаг за шагом. Игра ждёт твоего решения.", "One step at a time. The game waits for your decision.");
        Add("01  Alege un loc", "01  Выбери место", "01  Pick a spot");
        Add("Apasă pe o parcelă sau pe un post de lucru. Alege cultura ori contractul. Resursele lipsă se cumpără în același pas.",
            "Нажми на участок или рабочее место. Выбери культуру или контракт. Нехватка ресурсов докупается на том же шаге.",
            "Click a plot or a workstation. Choose the crop or the contract. Missing resources are bought in the same step.");
        Add("02  Încheie ziua", "02  Заверши день", "02  End the day");
        Add("Producția avansează cu o zi. Se plătesc întreținerea, salariile și rata creditului. Evenimentele îți arată efectele înainte de alegere.",
            "Производство продвигается на день. Оплачиваются обслуживание, зарплаты и платёж по кредиту. События показывают эффекты до выбора.",
            "Production advances by one day. Maintenance, salaries and the loan payment are due. Events show their effects before you choose.");
        Add("03  Încasează și crește", "03  Получай и расти", "03  Earn and grow");
        Add("Ferma păstrează recolta pentru piață. IT încasează automat la livrare. Investește apoi în ramurile de pe harta Dezvoltare.",
            "Ферма хранит урожай для рынка. IT получает оплату автоматически при доставке. Затем инвестируй в ветки карты развития.",
            "The farm keeps the harvest for the market. IT gets paid automatically on delivery. Then invest in the branches of the Development map.");
        Add("Câștigi cu {0:N0} {1} profit în cel mult {2} de zile. Un sold negativ înseamnă faliment.",
            "Ты выигрываешь с {0:N0} {1} прибыли максимум за {2} дней. Отрицательный баланс означает банкротство.",
            "You win with {0:N0} {1} profit in at most {2} days. A negative balance means bankruptcy.");
        Add("1-6: schimbă secțiunea · Esc: închide detaliile sau deschide pauza · Clic dreapta pe obiect: acțiuni",
            "1-6: смени раздел · Esc: закрой детали или открой паузу · правый клик по объекту: действия",
            "1-6: switch section · Esc: close details or open pause · right-click an object: actions");
        Add("Poți închide jocul și continua mai târziu. Salvarea este pe acest calculator.",
            "Можно закрыть игру и продолжить позже. Сохранение на этом компьютере.",
            "You can close the game and continue later. The save is on this computer.");
        Add("← Înapoi", "← Назад", "← Back");

        Add("Alege scenariul", "Выбери сценарий", "Choose the scenario");
        Add("Dificultate selectată: {0} - {1}", "Сложность: {0} - {1}", "Difficulty: {0} - {1}");
        Add("Costuri fixe pe zi", "Фиксированные расходы в день", "Fixed costs per day");
        Add("Joacă: {0}", "Играть: {0}", "Play: {0}");

        Add("Alege dificultatea", "Выбери сложность", "Choose the difficulty");
        Add("Condițiile financiare de start diferă - restul regulilor sunt identice.",
            "Стартовые финансовые условия разные - остальные правила одинаковы.",
            "Starting financial conditions differ - the rest of the rules are the same.");
        Add("Bani proprii", "Свои деньги", "Own money");
        Add("Grant", "Грант", "Grant");
        Add("Credit", "Кредит", "Loan");
        Add("Rambursare zilnică", "Ежедневный платёж", "Daily repayment");
        Add("Lichidități la start", "Ликвидности на старте", "Cash at start");
        Add("Profit țintă", "Целевая прибыль", "Target profit");
        Add("Pornește ({0})", "Начать ({0})", "Start ({0})");

        Add("Profitul față de țintă, zi cu zi", "Прибыль против цели, день за днём", "Profit vs target, day by day");
        Add("■ zi cu profit", "■ день с прибылью", "■ profitable day");
        Add("■ zi cu pierdere", "■ день с убытком", "■ losing day");
        Add("-- ținta {0}", "-- цель {0}", "-- target {0}");
        Add("Rezultatul financiar", "Финансовый итог", "Financial result");
        Add("Profit realizat", "Полученная прибыль", "Profit earned");
        Add("Țintă", "Цель", "Target");
        Add("Venituri totale", "Общие доходы", "Total income");
        Add("Cheltuieli totale", "Общие расходы", "Total expenses");
        Add("Sold final", "Итоговый баланс", "Final balance");
        Add("Credit rămas", "Остаток кредита", "Loan left");
        Add("Zile folosite", "Использовано дней", "Days used");
        Add("Dificultate", "Сложность", "Difficulty");
        Add("Scenariu", "Сценарий", "Scenario");
        Add("Joacă din nou", "Играть снова", "Play again");
        Add("Meniu principal", "Главное меню", "Main menu");

        Add("Alege un obiect din scena pentru productie.", "Выбери объект в сцене для производства.", "Pick an object in the scene to produce.");
        Add("Vinde recolta sau verifica ofertele si resursele.", "Продай урожай или проверь предложения и ресурсы.", "Sell the harvest or check offers and resources.");
        Add("Urmareste ramurile si deblocheaza imbunatatiri.", "Следи за ветками и открывай улучшения.", "Follow the branches and unlock upgrades.");
        Add("Angajeaza oameni si compara bonusul cu salariul.", "Нанимай людей и сравнивай бонус с зарплатой.", "Hire people and compare the bonus with the salary.");
        Add("Vezi banii, costurile, datoria si profitul.", "Смотри на деньги, расходы, долг и прибыль.", "See the money, costs, debt and profit.");
        Add("Urmareste obiectivele si ultimele actiuni.", "Следи за целями и последними действиями.", "Track the objectives and recent actions.");
        Add("Afacerea", "Бизнес", "Business");
        Add("Piață", "Рынок", "Market");
        Add("Dezvoltare", "Развитие", "Development");
        Add("Echipă", "Команда", "Team");
        Add("Finanțe", "Финансы", "Finance");
        Add("Jurnal", "Журнал", "Journal");
        Add("FERMA MEA", "МОЯ ФЕРМА", "MY FARM");
        Add("STUDIOUL MEU", "МОЙ СТУДИО", "MY STUDIO");
        Add("Profitul realizat fata de obiectivul campaniei.", "Прибыль относительно цели кампании.", "Profit against the campaign target.");
        Add("Meniu", "Меню", "Menu");
        Add("Pauza, salvare si meniul principal. Esc: inchide mai intai panoul activ.",
            "Пауза, сохранение и главное меню. Esc: сначала закрой активную панель.",
            "Pause, save and main menu. Esc: close the active panel first.");
        Add("Încheie ziua", "Завершить день", "End the day");
        Add("Avanseaza o zi: productia progreseaza si se platesc costurile zilnice.",
            "Продвижение на день: производство растёт, оплачиваются дневные расходы.",
            "Advance one day: production progresses and daily costs are paid.");
        Add("Clic pe obiect: producție  |  Clic dreapta: acțiuni  |  1-6: meniuri",
            "Клик по объекту: производство  |  правый клик: действия  |  1-6: меню",
            "Click an object: production  |  right-click: actions  |  1-6: menus");

        Add("Ziua {0} / {1} · {2}", "День {0} / {1} · {2}", "Day {0} / {1} · {2}");
        Add("Cost zilnic {0:N0} + rată credit {1:N0} {2}", "Дневные расходы {0:N0} + платёж по кредиту {1:N0} {2}", "Daily cost {0:N0} + loan payment {1:N0} {2}");
        Add("Profit {0:N0} / {1:N0} {2}", "Прибыль {0:N0} / {1:N0} {2}", "Profit {0:N0} / {1:N0} {2}");
        Add("Salvarea nu a reușit. Eliberează spațiu și încearcă din nou înainte de a ieși.",
            "Не удалось сохранить. Освободи место и попробуй снова перед выходом.",
            "Saving failed. Free up space and try again before exiting.");
        Add("Salvarea nu a reușit. Jocul rămâne deschis pentru a putea încerca din nou.",
            "Не удалось сохранить. Игра остаётся открытой, чтобы попробовать снова.",
            "Saving failed. The game stays open so you can try again.");

        Add("Recolta este gata", "Урожай готов", "The harvest is ready");
        Add("Lucrări în curs", "Работы в процессе", "Work in progress");
        Add("Alege o parcelă", "Выбери участок", "Pick a plot");
        Add("Alege un post", "Выбери место", "Pick a workstation");
        Add("Ai marfă în depozit. Vinde pe piață pentru a încasa venitul.",
            "У тебя есть товар на складе. Продай на рынке, чтобы получить доход.",
            "You have goods in storage. Sell them on the market to collect the income.");
        Add("Încheie ziua pentru a avansa lucrările. Timpul așteaptă alegerea ta.",
            "Заверши день, чтобы продвинуть работы. Время ждёт твоего выбора.",
            "End the day to advance the work. Time waits for your choice.");
        Add("Clic pe teren pentru a planta. Recolta se vinde pe piață.",
            "Кликни на землю, чтобы посадить. Урожай продаётся на рынке.",
            "Click the land to plant. The harvest is sold at the market.");
        Add("Clic pe un birou pentru un contract. Plata vine la livrare.",
            "Кликни на рабочее место для контракта. Оплата приходит при доставке.",
            "Click a desk for a contract. Payment comes on delivery.");
        Add("Vinde recolta", "Продать урожай", "Sell the harvest");
        Add("Arată ghidul", "Показать руководство", "Show the guide");

        Add("FERMĂ  /  ADMINISTRARE", "ФЕРМА  /  УПРАВЛЕНИЕ", "FARM  /  MANAGEMENT");
        Add("STUDIO IT  /  ADMINISTRARE", "IT-СТУДИО  /  УПРАВЛЕНИЕ", "IT STUDIO  /  MANAGEMENT");
        Add("Inchide si revino la scena (Esc).", "Закрой и вернись к сцене (Esc).", "Close and return to the scene (Esc).");
        Add("Derulează pentru detalii  ·  Esc: închide  ·  Bara de jos schimbă meniul",
            "Прокрути для деталей  ·  Esc: закрыть  ·  Нижняя панель меняет меню",
            "Scroll for details  ·  Esc: close  ·  The bottom bar switches menus");

        Add("Parcelă {0}", "Участок {0}", "Plot {0}");
        Add("Post de lucru {0}", "Рабочее место {0}", "Workstation {0}");
        Add("Extinde afacerea pentru încă un loc de producție.",
            "Расширь бизнес ещё на одно место производства.",
            "Expand the business for one more production slot.");
        Add("Vezi harta dezvoltării", "Открыть карту развития", "Open the development map");
        Add("{0} loturi · {1} zile până la livrare", "{0} партий · {1} дней до доставки", "{0} batches · {1} days until delivery");
        Add("Plata se încasează automat la livrare.", "Оплата поступает автоматически при доставке.", "Payment is collected automatically on delivery.");
        Add("Vei recolta {0} unități. Apoi le vinzi pe piață.", "Ты соберёшь {0} единиц. Затем продашь их на рынке.", "You will harvest {0} units. Then you sell them at the market.");
        Add("Timpul avansează numai când închei ziua.", "Время идёт только когда ты завершаешь день.", "Time advances only when you end the day.");
        Add("Alege ce plantezi pe acest teren.", "Выбери, что посадить на этом участке.", "Choose what to plant on this land.");
        Add("Alege contractul pentru acest post.", "Выбери контракт для этого места.", "Choose the contract for this workstation.");
        Add("Loturi", "Партии", "Batches");
        Add("{0} zile · estimat {1:N0} {2}", "{0} дней · оценка {1:N0} {2}", "{0} days · est. {1:N0} {2}");
        Add("{0} resurse lipsă", "Не хватает {0} ресурсов", "{0} resources missing");
        Add("{0} zile", "{0} дней", "{0} days");
        Add("Acceptă · {0:N0} {1}", "Принять · {0:N0} {1}", "Accept · {0:N0} {1}");
        Add("Plantează · {0:N0} {1}", "Посадить · {0:N0} {1}", "Plant · {0:N0} {1}");
        Add("{0}: producția a început.", "{0}: производство началось.", "{0}: production started.");
        Add("Include {0} resurse lipsă. Prețul final de vânzare poate varia.",
            "Учитывает нехватку ресурсов: {0}. Итоговая цена продажи может меняться.",
            "Includes {0} missing resources. The final sale price may vary.");
        Add("Fonduri insuficiente: lipsesc {0:N0} {1}.", "Недостаточно средств: не хватает {0:N0} {1}.", "Insufficient funds: {0:N0} {1} short.");

        Add("GHIDUL TĂU", "ТВОЁ РУКОВОДСТВО", "YOUR GUIDE");
        Add("Inchide ghidul. Il poti redeschide din scena.", "Закрой руководство. Его можно открыть снова из сцены.", "Close the guide. You can reopen it from the scene.");
        Add("URMĂTORUL PAS", "СЛЕДУЮЩИЙ ШАГ", "NEXT STEP");
        Add("Pune afacerea în mișcare", "Запусти бизнес", "Get the business moving");
        Add("Lucrările sunt în curs", "Работы идут", "Work is in progress");
        Add("Apasă pe un loc liber din scenă. Alege producția, iar resursele necesare se cumpără împreună cu ea.",
            "Нажми на свободное место в сцене. Выбери производство, недостающие ресурсы купятся вместе с ним.",
            "Click a free spot in the scene. Choose the production; the needed resources are bought with it.");
        Add("Încheie ziua pentru a avansa producția. Costurile zilnice se plătesc la fiecare pas.",
            "Заверши день, чтобы продвинуть производство. Дневные расходы оплачиваются на каждом шагу.",
            "End the day to advance production. Daily costs are paid at every step.");
        Add("Deschide piața", "Открыть рынок", "Open the market");
        Add("Alege un loc liber", "Выбери свободное место", "Pick a free spot");
        Add("Avansează producția", "Продвинуть производство", "Advance production");
        Add("Locuri active", "Активные места", "Active slots");
        Add("Costuri / zi", "Расходы / день", "Costs / day");
        Add("Rată / zi", "Платёж / день", "Payment / day");
        Add("PRIMII PAȘI", "ПЕРВЫЕ ШАГИ", "FIRST STEPS");
        Add("Ascunde ghidul", "Скрыть руководство", "Hide the guide");
        Add("Clic dreapta pe obiect: acțiuni. Dezvoltare: îmbunătățiri legate între ele.",
            "Правый клик по объекту: действия. Развитие: связанные между собой улучшения.",
            "Right-click an object: actions. Development: upgrades linked to each other.");

        Add("Contractele se încasează automat. Aici vezi ofertele și cumperi resurse în avans.",
            "Контракты оплачиваются автоматически. Здесь ты видишь предложения и покупаешь ресурсы заранее.",
            "Contracts are paid automatically. Here you see offers and buy resources in advance.");
        Add("Prețurile se schimbă zilnic. Marfa rămâne în depozit până alegi să vinzi.",
            "Цены меняются ежедневно. Товар остаётся на складе, пока ты не решишь продать.",
            "Prices change daily. Goods stay in storage until you choose to sell.");
        Add("{0}: resurse {1} {2}, durata {3} zile.", "{0}: ресурсы {1} {2}, длительность {3} дней.", "{0}: resources {1} {2}, duration {3} days.");
        Add("per contract livrat", "за доставленный контракт", "per delivered contract");
        Add("per unitate", "за единицу", "per unit");
        Add("{0:+0;-0;0}% față de prețul de bază", "{0:+0;-0;0}% от базовой цены", "{0:+0;-0;0}% vs base price");
        Add("În depozit", "На складе", "In storage");
        Add("Vinde tot · {0:N0} {1}", "Продать всё · {0:N0} {1}", "Sell all · {0:N0} {1}");
        Add("Marfa a fost vândută.", "Товар продан.", "The goods were sold.");
        Add("Durată actuală: {0} zile", "Текущая длительность: {0} дней", "Current duration: {0} days");
        Add("Resurse în avans", "Ресурсы заранее", "Resources in advance");
        Add("Opțional: producția cumpără automat numai resursele lipsă. Cumpără aici dacă vrei să pregătești stocul.",
            "По желанию: производство докупает только нехватку ресурсов. Купи здесь, если хочешь подготовить запас.",
            "Optional: production automatically buys only the missing resources. Buy here if you want to prepare stock.");
        Add("Stoc: {0}", "Запас: {0}", "Stock: {0}");
        Add("Cumpără 1 · {0} {1}", "Купить 1 · {0} {1}", "Buy 1 · {0} {1}");

        Add("Alege o ramură. Nivelul al doilea cere îmbunătățirea din stânga. Efectele se aplică imediat.",
            "Выбери ветку. Второй уровень требует улучшение слева. Эффекты применяются сразу.",
            "Pick a branch. The second level requires the upgrade on the left. Effects apply immediately.");
        Add("01  Productivitate", "01  Производительность", "01  Productivity");
        Add("02  Capacitate", "02  Ёмкость", "02  Capacity");
        Add("03  Valoare și eficiență", "03  Стоимость и эффективность", "03  Value and efficiency");
        Add("Deținut", "Куплено", "Owned");
        Add("Blocat", "Заблокировано", "Locked");
        Add("Dezvoltă · {0:N0} {1}", "Развить · {0:N0} {1}", "Develop · {0:N0} {1}");
        Add("Îmbunătățire activată: {0}", "Улучшение активировано: {0}", "Upgrade activated: {0}");
        Add("Activ permanent", "Активно навсегда", "Active forever");
        Add("Necesită: {0}", "Требует: {0}", "Requires: {0}");
        Add("Mai ai nevoie de {0:N0} {1}", "Ещё нужно {0:N0} {1}", "You still need {0:N0} {1}");
        Add("Disponibil acum", "Доступно сейчас", "Available now");

        Add("Bonusurile au un salariu zilnic. Angajarea cere un salariu în avans.",
            "Бонусы дают дневную зарплату. Найм требует зарплату вперёд.",
            "Bonuses carry a daily salary. Hiring requires a salary upfront.");
        Add("{0} {1}/zi · Angajați: {2}", "{0} {1}/день · Нанято: {2}", "{0} {1}/day · Hired: {2}");
        Add("Angajează", "Нанять", "Hire");
        Add("Platesti un salariu acum, apoi salariul zilnic. Bonusul se aplica imediat.",
            "Сначала платишь зарплату, потом - ежедневно. Бонус действует сразу.",
            "You pay a salary now, then the daily salary. The bonus applies immediately.");
        Add("Concediază", "Уволить", "Fire");
        Add("Cost zilnic cu personal și întreținere", "Дневные расходы на персонал и обслуживание", "Daily cost with staff and maintenance");

        Add("Soldul este suma disponibilă. Profitul scade datoria rămasă și capitalul inițial, inclusiv grantul.",
            "Баланс - это доступная сумма. Прибыль учитывает остаток долга и начальный капитал, включая грант.",
            "The balance is the available amount. Profit subtracts the remaining debt and the initial capital, including the grant.");
        Add("Sold disponibil", "Доступный баланс", "Available balance");
        Add("Datorie rămasă", "Остаток долга", "Remaining debt");
        Add("Capital inițial", "Начальный капитал", "Initial capital");
        Add("Plata estimată la sfârșitul zilei", "Ожидаемый платёж в конце дня", "Estimated payment at day end");
        Add("Rezervă fără venituri: circa {0} zile la costul actual.",
            "Запас без доходов: около {0} дней при текущих расходах.",
            "Runway without income: about {0} days at the current cost.");
        Add("Fără costuri zilnice.", "Без дневных расходов.", "No daily costs.");
        Add("Efecte active", "Активные эффекты", "Active effects");
        Add("Niciun efect temporar.", "Нет временных эффектов.", "No temporary effects.");
        Add("Preț de vânzare", "Цена продажи", "Sale price");
        Add("Cost resurse", "Стоимость ресурсов", "Resource cost");
        Add("Întreținere", "Обслуживание", "Maintenance");
        Add("{0} {1:+0%;-0%} · {2} zile", "{0} {1:+0%;-0%} · {2} дней", "{0} {1:+0%;-0%} · {2} days");

        Add("Obiective · {0} / {1}", "Цели · {0} / {1}", "Objectives · {0} / {1}");
        Add("Obiective atinse in partida curenta.", "Достигнутые цели в текущей партии.", "Objectives reached in the current game.");
        Add("Ultimele acțiuni", "Последние действия", "Recent actions");

        Add("Închei ziua?", "Завершить день?", "End the day?");
        Add("Nu ai producție activă. Vei plăti costurile zilnice fără ca o lucrare să avanseze.",
            "У тебя нет активного производства. Ты заплатишь дневные расходы, не продвинув ни одну работу.",
            "You have no active production. You will pay daily costs without any work advancing.");
        Add("Soldul actual nu acoperă plățile estimate. Livrările IT de astăzi pot aduce venit; un sold final negativ înseamnă faliment.",
            "Текущий баланс не покрывает ожидаемые платежи. Сегодняшние IT-доставки могут принести доход; отрицательный итоговый баланс означает банкротство.",
            "The current balance does not cover the estimated payments. Today's IT deliveries can bring income; a negative final balance means bankruptcy.");
        Add("Continuă ziua următoare", "Перейти к следующему дню", "Continue to the next day");
        Add("Înapoi la afacere", "Назад к бизнесу", "Back to business");
        Add("Zi încheiată · venit +{0:N0} · cheltuieli -{1:N0} · credit -{2:N0} {3}",
            "День завершён · доход +{0:N0} · расходы -{1:N0} · кредит -{2:N0} {3}",
            "Day ended · income +{0:N0} · expenses -{1:N0} · loan -{2:N0} {3}");
        Add("Partidă în pauză", "Игра на паузе", "Game paused");
        Add("Timpul avansează numai la încheierea zilei. Progresul se salvează local automat.",
            "Время идёт только при завершении дня. Прогресс сохраняется локально автоматически.",
            "Time advances only at the end of the day. Progress is saved locally automatically.");
        Add("Continuă jocul", "Продолжить игру", "Resume game");
        Add("Salvează și mergi la meniu", "Сохранить и выйти в меню", "Save and go to menu");

        Add("Am înțeles", "Понятно", "Got it");
        Add("Bani {0:+0;-0}", "Деньги {0:+0;-0}", "Money {0:+0;-0}");
        Add("Preț {0:+0%;-0%}", "Цена {0:+0%;-0%}", "Price {0:+0%;-0%}");
        Add("Resurse {0:+0%;-0%}", "Ресурсы {0:+0%;-0%}", "Resources {0:+0%;-0%}");
        Add("Cost zilnic {0:+0%;-0%}", "Дневные расходы {0:+0%;-0%}", "Daily cost {0:+0%;-0%}");
        Add("Fără cost suplimentar", "Без допрасходов", "No extra cost");

        Add("Contracte", "Контракты", "Contracts");
        Add("FERMĂ / ACȚIUNI", "ФЕРМА / ДЕЙСТВИЯ", "FARM / ACTIONS");
        Add("STUDIO IT / ACȚIUNI", "IT-СТУДИО / ДЕЙСТВИЯ", "IT STUDIO / ACTIONS");
        Add("Gestionează obiectul", "Управлять объектом", "Manage the object");
        Add("Harta dezvoltării", "Карта развития", "Development map");
        Add("Piață și resurse", "Рынок и ресурсы", "Market and resources");
        Add("Esc sau clic în afară: închide", "Esc или клик мимо: закрыть", "Esc or click outside: close");
        Add("Clic pentru acțiuni. Clic dreapta pentru meniul contextual.",
            "Клик - действия. Правый клик - контекстное меню.",
            "Click for actions. Right-click for the context menu.");
        Add("Micșorează camera", "Отдалить камеру", "Zoom out the camera");
        Add("Mărește camera", "Приблизить камеру", "Zoom in the camera");
        Add("Rotire camera", "Поворот камеры", "Camera rotation");
        Add("Centrare camera", "Центрировать камеру", "Center the camera");
        Add("+ Extindere", "+ Расширение", "+ Extension");
        Add("Parcelă {0}  +", "Участок {0}  +", "Plot {0}  +");
        Add("Post {0}  +", "Место {0}  +", "Desk {0}  +");
        Add("{0} · {1}z", "{0} · {1} дн", "{0} · {1}d");

        Add("Faliment", "Банкротство", "Bankruptcy");
        Add("Soldul a devenit negativ. Bancherii ți-au luat cheile de la afacere.",
            "Баланс стал отрицательным. Банкиры забрали ключи от твоего бизнеса.",
            "The balance turned negative. The bankers took the keys to your business.");
        Add("Ținta atinsă!", "Цель достигнута!", "Target reached!");
        Add("Timpul a expirat", "Время вышло", "Time is up");
        Add("Ai obținut {0} {1} profit în {2} zile. Felicitări!",
            "Получено {0} {1} прибыли за {2} дней. Поздравляем!",
            "Earned {0} {1} profit in {2} days. Congratulations!");
        Add("Ai încheiat cu {0} {1} profit, iar ținta era de {2} {1}.",
            "Закончил с прибылью {0} {1}, цель была {2} {1}.",
            "Finished with {0} {1} profit; the target was {2} {1}.");

        Add("Prima producție", "Первое производство", "First production");
        Add("Pornește o cultură sau un contract.", "Запусти культуру или контракт.", "Start a crop or a contract.");
        Add("Prima livrare", "Первая доставка", "First delivery");
        Add("Încheie zilele până când o lucrare este gata.", "Завершай дни, пока работа не готова.", "End days until a job is done.");
        Add("Primul venit", "Первый доход", "First income");
        Add("Vinde recolta; contractele IT se plătesc la livrare.", "Продай урожай; IT-контракты платят при доставке.", "Sell the harvest; IT contracts pay on delivery.");
        Add("Afacere în creștere", "Растущий бизнес", "Growing business");
        Add("Cumpără o îmbunătățire de pe harta dezvoltării.", "Купи улучшение на карте развития.", "Buy an upgrade from the development map.");
        Add("Pe profit", "В плюсе", "In profit");
        Add("Acoperă cheltuielile și depășește capitalul inițial.", "Покрой расходы и превыси начальный капитал.", "Cover the expenses and beat the initial capital.");

        Add("Acțiunea nu este disponibilă acum.", "Действие сейчас недоступно.", "This action is not available right now.");
        Add("Toate locurile sunt ocupate.", "Все места заняты.", "All slots are taken.");
        Add("Locul ales nu este liber.", "Выбранное место не свободно.", "The chosen slot is not free.");
        Add("Fonduri insuficiente pentru resurse.", "Недостаточно средств на ресурсы.", "Insufficient funds for resources.");
        Add("Jocul s-a încheiat.", "Игра окончена.", "The game is over.");
        Add("Rezolvă mai întâi evenimentul.", "Сначала реши событие.", "Resolve the event first.");
        Add("Fonduri insuficiente.", "Недостаточно средств.", "Insufficient funds.");
        Add("Nu ai marfă pe stoc.", "У тебя нет товара на складе.", "You have no goods in stock.");
        Add("Cumpără mai întâi îmbunătățirea anterioară de pe hartă.", "Сначала купи предыдущее улучшение на карте.", "Buy the previous upgrade on the map first.");
        Add("Deja deținut.", "Уже куплено.", "Already owned.");
        Add("Fonduri insuficiente pentru salariu.", "Недостаточно средств на зарплату.", "Insufficient funds for the salary.");
        Add("Maximum {0} loturi pe loc.", "Максимум {0} партий на место.", "Maximum {0} batches per slot.");
        Add("Nu mai ai {0} libere ({1}/{2}).", "Все {0} заняты ({1}/{2}).", "All {0} are busy ({1}/{2}).");
        Add("Nu ai suficiente resurse pentru {0}.", "Недостаточно ресурсов для {0}.", "Not enough resources for {0}.");
        Add("parcele de teren", "участки земли", "land plots");
        Add("contracte simultane", "одновременные контракты", "concurrent contracts");

        Add("[Ziua {0}]", "[День {0}]", "[Day {0}]");
        Add("Ai început: {0} × {1} ({2} {3})", "Начато: {0} × {1} ({2} {3})", "Started: {0} × {1} ({2} {3})");
        Add("zi", "дн.", "day");
        Add("zile", "дн.", "days");
        Add("Ai cumpărat: {0}.", "Куплено: {0}.", "Bought: {0}.");
        Add("Ai angajat: {0} ({1} bucăți, {2} {3}/zi).", "Нанят: {0} ({1} чел., {2} {3}/день).", "Hired: {0} ({1}, {2} {3}/day).");
        Add("Ai concediat: {0}.", "Уволен: {0}.", "Fired: {0}.");
        Add("Recoltă gata: +{0} {1} pe stoc.", "Урожай готов: +{0} {1} на складе.", "Harvest ready: +{0} {1} in stock.");
        Add("Cumpărat {0} × {1}", "Куплено {0} × {1}", "Bought {0} × {1}");
        Add("Vândut {0} × {1} la {2} {3}/buc", "Продано {0} × {1} по {2} {3}/шт.", "Sold {0} × {1} at {2} {3}/unit");
        Add("Îmbunătățire: {0}", "Улучшение: {0}", "Upgrade: {0}");
        Add("Angajat: {0} (avans)", "Нанят: {0} (аванс)", "Hired: {0} (upfront)");
        Add("Contract livrat: {0} × {1}", "Контракт выполнен: {0} × {1}", "Contract delivered: {0} × {1}");
        Add("Eveniment", "Событие", "Event");
        Add("rambursare credit (mai ai {0} {1})", "погашение кредита (осталось {0} {1})", "loan repayment ({0} {1} left)");
        Add("Ai început afacerea „{0}” pe dificultatea {1}.", "Начал бизнес „{0}” на сложности {1}.", "Started the business „{0}” on {1} difficulty.");
        Add("Ai primit un grant de {0} {1}.", "Получен грант: {0} {1}.", "Received a grant of {0} {1}.");
        Add("Ai luat un credit de {0} {1}, cu rambursare de {2} {3}/zi.", "Взят кредит {0} {1}, погашение {2} {3}/день.", "Took a loan of {0} {1}, repaying {2} {3}/day.");
        Add("Ținta: {0} {1} profit în {2} zile.", "Цель: {0} {1} прибыли за {2} дней.", "Target: {0} {1} profit in {2} days.");

        Add("Ferma Agricolă", "Аграрная ферма", "Agricultural Farm");
        Add("Agricultură", "Сельское хозяйство", "Agriculture");
        Add("Cultivă plante, cumpără resurse, recoltează și vinde producția pe piață.",
            "Выращивай растения, покупай ресурсы, собирай и продавай продукцию на рынке.",
            "Grow plants, buy resources, harvest and sell produce at the market.");
        Add("Întreținere zilnică (mentenanță, utilități)", "Ежедневное обслуживание (ремонт, эксплуатация)", "Daily upkeep (maintenance, utilities)");
        Add("Parcele de teren", "Участки земли", "Land plots");
        Add("Companie IT", "IT-компания", "IT Company");
        Add("Angajează personal, ia contracte, dezvoltă servicii și încasează veniturile.",
            "Нанимай людей, бери контракты, развивай услуги и получай доход.",
            "Hire staff, take contracts, develop services and collect the revenue.");
        Add("Chirie, servere și utilități", "Аренда, серверы и коммунальные", "Rent, servers and utilities");
        Add("Contracte simultane", "Одновременные контракты", "Concurrent contracts");
        Add("Ușor", "Лёгкий", "Easy");
        Add("Mediu", "Средний", "Medium");
        Add("Greu", "Сложный", "Hard");
        Add("Complex", "Сложный", "Hard");
        Add("Afacerea beneficiază de un grant care acoperă o parte din costurile inițiale.",
            "Бизнес получает грант, покрывающий часть начальных расходов.",
            "The business gets a grant covering part of the initial costs.");
        Add("Afacerea este finanțată exclusiv din banii proprii.",
            "Бизнес финансируется только из личных денег.",
            "The business is funded entirely from your own money.");
        Add("Afacerea începe cu un credit care trebuie rambursat în fiecare zi.",
            "Бизнес начинается с кредита, который гасят каждый день.",
            "The business starts with a loan that must be repaid daily.");

        Add("Sămânță grâu", "Семена пшеницы", "Wheat seeds");
        Add("Sămânță porumb", "Семена кукурузы", "Corn seeds");
        Add("Răsaduri legume", "Рассада овощей", "Vegetable seedlings");
        Add("Grâu", "Пшеница", "Wheat");
        Add("Porumb", "Кукуруза", "Corn");
        Add("Legume", "Овощи", "Vegetables");
        Add("Sistem de irigații", "Система полива", "Irrigation system");
        Add("Reducere cu 1 zi a timpului de producție.", "Сокращение времени производства на 1 день.", "Reduces production time by 1 day.");
        Add("Îngrășăminte", "Удобрения", "Fertilizer");
        Add("+1 unitate la fiecare recoltă.", "+1 единица к каждому урожаю.", "+1 unit per harvest.");
        Add("Tractor nou", "Новый трактор", "New tractor");
        Add("Resursele costă cu 20% mai puțin.", "Ресурсы дешевле на 20%.", "Resources cost 20% less.");
        Add("Certificare bio", "Биосертификация", "Organic certification");
        Add("Preț de vânzare cu 15% mai mare.", "Цена продажи выше на 15%.", "Selling price 15% higher.");
        Add("Teren suplimentar", "Дополнительный участок", "Extra land");
        Add("+1 parcelă de cultură (producție simultană).", "+1 участок для посева (параллельное производство).", "+1 plot (simultaneous production).");
        Add("Panouri solare", "Солнечные панели", "Solar panels");
        Add("Costurile fixe zilnice scad cu 30%.", "Фиксированные расходы ниже на 30%.", "Fixed daily costs drop by 30%.");
        Add("Zilier", "Подёнщик", "Day laborer");
        Add("Agronom", "Агроном", "Agronomist");

        Add("Echipament și licențe", "Оборудование и лицензии", "Equipment and licenses");
        Add("Server cloud", "Облачный сервер", "Cloud server");
        Add("Site de prezentare", "Сайт-визитка", "Landing site");
        Add("Mentenanță lunară", "Ежемесячное обслуживание", "Monthly maintenance");
        Add("Aplicație mobilă", "Мобильное приложение", "Mobile app");
        Add("Calculatoare noi", "Новые компьютеры", "New computers");
        Add("Reducere cu 1 zi a duratei contractelor.", "Сокращение длительности контрактов на 1 день.", "Reduces contract duration by 1 day.");
        Add("Automatizare (CI/CD)", "Автоматизация (CI/CD)", "Automation (CI/CD)");
        Add("Încă 1 zi redusă din durata contractelor.", "Ещё минус 1 день к длительности контрактов.", "One more day off contract duration.");
        Add("Licențe în volum", "Лицензии оптом", "Volume licenses");
        Add("Echipamentul costă cu 20% mai puțin.", "Оборудование дешевле на 20%.", "Equipment costs 20% less.");
        Add("Marketing digital", "Цифровой маркетинг", "Digital marketing");
        Add("Valoarea contractelor crește cu 15%.", "Стоимость контрактов выше на 15%.", "Contract value rises by 15%.");
        Add("Echipă suplimentară", "Дополнительная команда", "Extra team");
        Add("+1 contract simultan.", "+1 одновременный контракт.", "+1 concurrent contract.");
        Add("Birou remote", "Удалённый офис", "Remote office");
        Add("Costurile fixe zilnice scad cu 25%.", "Фиксированные расходы ниже на 25%.", "Fixed daily costs drop by 25%.");
        Add("Dezvoltator junior", "Джун-разработчик", "Junior developer");
        Add("Designer UI/UX", "UI/UX-дизайнер", "UI/UX designer");
        Add("Valoarea contractelor crește cu 8%.", "Стоимость контрактов выше на 8%.", "Contract value rises by 8%.");
        Add("Manager vânzări", "Менеджер по продажам", "Sales manager");
        Add("Valoarea contractelor crește cu 12%.", "Стоимость контрактов выше на 12%.", "Contract value rises by 12%.");

        Add("Motorina se scumpește peste noapte", "Дизель дорожает за ночь", "Diesel fuel jumps overnight");
        Add("Posterioarele de la benzină au afișat un preț nou dimineața. Transportul și utilajele costă mai mult o perioadă.",
            "На заправках утром уже новая цена. Транспорт и техника какое-то время дороже.",
            "The pumps showed a new price in the morning. Transport and equipment cost more for a while.");
        Add("Suportă costul (resurse mai scumpe)", "Повыси расходы (ресурсы дорожают)", "Absorb the cost (dearer resources)");
        Add("Renunți la transport o perioadă (-400)", "Отказ от транспорта на время (-400)", "Skip transport for a while (-400)");
        Add("Blocaj la frontieră", "Пробки на границе", "Border jam");
        Add("Camioanele cu marfă stau la coadă de trei zile. Cererea scade, iar prețurile pe piață o iau în jos.",
            "Грузовики стоят в очереди третий день. Спрос падает, цены на рынке идут вниз.",
            "Trucks have queued for three days. Demand drops and market prices slide.");
        Add("Vreme favorabilă, piață inflamată", "Хорошая погода, горячий рынок", "Good weather, hot market");
        Add("Toată lumea cumpără în panică înainte de iarnă. Prețurile la produsele tale cresc.",
            "Все скупают запасы перед зимой. Цены на твои товары растут.",
            "Everyone panic-buys before winter. Prices for your goods rise.");
        Add("Avarie la utilaje", "Поломка техники", "Equipment breakdown");
        Add("Utilajul s-a stricat exact în ziua de plată. Reparația costă, dar amânia costă mai mult.",
            "Техника сломалась в день платежа. Ремонт стоит денег, но откладывать дороже.",
            "The equipment broke on payment day. Repairs cost, but postponing costs more.");
        Add("Reparație urgentă (-700)", "Срочный ремонт (-700)", "Urgent repair (-700)");
        Add("Amâni reparația (costuri fixe mai mari)", "Отложить ремонт (фикс. расходы выше)", "Postpone the repair (higher fixed costs)");
        Add("Program de susținere a agriculturii", "Программа поддержки сельского хозяйства", "Agriculture support program");
        Add("A apărut o linie de finanțare pentru micii producători. Cererea se depune repede, banii vin și mai repede.",
            "Появилось финансирование для мелких производителей. Заявка подаётся быстро, деньги приходят ещё быстрее.",
            "A funding line appeared for small producers. Apply fast; the money comes even faster.");
        Add("Reduceri de licențe", "Скидки на лицензии", "License discounts");
        Add("Furnizorul de licențe are o ofertă „pentru că e sfârșit de trimestru”. Echipamentul devine mai ieftin.",
            "Поставщик лицензий даёт скидку „потому что конец квартала”. Оборудование дешевеет.",
            "The license supplier has an offer „because it is quarter end”. Equipment gets cheaper.");
        Add("Contract de la un client din afară", "Контракт от иностранного клиента", "Contract from an overseas client");
        Add("O companie din offshore îți trimite un avans. Actele sunt „în curs de pregătire”, banii sunt reali.",
            "Оффшорная компания присылает аванс. Документы „в процессе”, а деньги настоящие.",
            "An offshore company sends an advance. The papers are „being prepared”; the money is real.");
        Add("Infecție cu ransomware", "Рансомвар", "Ransomware infection");
        Add("Un e-mail cu „factură atașată” a făcut ravagii. Fie plătești recuperarea, fie pierzi proiectele.",
            "Письмо со „счётом-вложением” натворило бед. Плати за восстановление или теряешь проекты.",
            "An email with an „attached invoice” did damage. Either pay for recovery or lose the projects.");
        Add("Recuperare plătită (-900)", "Платное восстановление (-900)", "Paid recovery (-900)");
        Add("Pierzi datele (-1800)", "Данные потеряны (-1800)", "You lose the data (-1800)");
        Add("A apărut un concurent", "Появился конкурент", "A competitor appears");
        Add("O firmă nouă vinde aceleași servicii cu 20% mai ieftin. Contractele tale valorează mai puțin o perioadă.",
            "Новая фирма продаёт те же услуги на 20% дешевле. Твои контракты какое-то время стоят меньше.",
            "A new firm sells the same services 20% cheaper. Your contracts are worth less for a while.");
        Add("Val de digitalizare", "Волна цифровизации", "Digitalization wave");
        Add("Toată lumea vrea „un site și o aplicație”. Valoarea contractelor crește temporar.",
            "Все хотят „сайт и приложение”. Стоимость контрактов временно растёт.",
            "Everyone wants „a site and an app”. Contract value rises temporarily.");
        Add("Control fiscal", "Налоговая проверка", "Tax audit");
        Add("Inspectorii au sosit fără anunțare și au găsit „nereguli minore” în acte. Ai de ales între avocat și amendă.",
            "Инспекторы приехали без предупреждения и нашли „мелкие нарушения” в документах. Выбор: адвокат или штраф.",
            "Inspectors arrived unannounced and found „minor irregularities” in the papers. Choose between a lawyer and a fine.");
        Add("Avocat și contestație (-600)", "Адвокат и апелляция (-600)", "Lawyer and appeal (-600)");
        Add("Plătești amenda (-1500)", "Платишь штраф (-1500)", "You pay the fine (-1500)");
        Add("Inflația își face de cap", "Инфляция бушует", "Inflation runs wild");
        Add("Prețurile cresc peste tot, inclusiv facturile tale. Costurile fixe devin mai grele.",
            "Цены растут везде, включая твои счета. Фиксированные расходы тяжелее.",
            "Prices rise everywhere, including your bills. Fixed costs get heavier.");
        Add("Cursul leului se depreciază", "Лей обесценивается", "The leu depreciates");
        Add("Leul pierde teren în fața euro. Exportatorii se bucură, cumpărătorii oftează - cererea crește.",
            "Лей теряет позиции к евро. Экспортёры радуются, покупатели вздыхают - спрос растёт.",
            "The leu loses ground against the euro. Exporters rejoice, buyers sigh - demand grows.");
        Add("Factură neașteptată la utilități", "Неожиданный счёт за услуги", "Unexpected utility bill");
        Add("Ai uitat de o factură veche. O plătești acum sau ea devine o „investiție” în penalizări.",
            "Ты забыл о старом счёте. Платишь сейчас или он становится „инвестицией” в штрафы.",
            "You forgot an old bill. Pay it now or it becomes an „investment” in penalties.");
        Add("Târg de profil", "Отраслевая ярмарка", "Trade fair");
        Add("Se organizează un târg local. Participarea costă, dar aduce vizibilitate și clienți.",
            "Проводится местная ярмарка. Участие стоит денег, но даёт видимость и клиентов.",
            "A local fair is being held. Taking part costs, but brings visibility and customers.");
        Add("Participi (-500, prețuri mai bune)", "Участвуешь (-500, лучшие цены)", "Take part (-500, better prices)");
        Add("Stai acasă", "Сидишь дома", "Stay home");
        Add("Apel de grant pentru tineri antreprenori", "Грант для молодых предпринимателей", "Grant call for young entrepreneurs");
        Add("Ai completat formularul „în ultima zi” și, minune, ai trecut. Banii intră în cont.",
            "Ты заполнил формуляр „в последний день” и, чудо, прошёл. Деньги поступают на счёт.",
            "You filled the form „on the last day” and, miracle, you passed. The money lands in the account.");
        Add("Banca majorează dobânzile", "Банк повышает ставки", "The bank raises interest rates");
        Add("Consiliul de administrație a decis: ratele cresc. Costurile tale fixe urcă și ele.",
            "Совет директоров решил: ставки растут. И твои фиксированные расходы поднимаются.",
            "The board decided: rates go up. Your fixed costs rise too.");
        Add("Întârziere în aprovizionare", "Задержка поставки", "Supply delay");
        Add("Livrarea de resurse întârzie o săptămână. Ce ai în stoc e acum mai valoros pe piață.",
            "Поставка ресурсов задерживается на неделю. То, что на складе, теперь дороже на рынке.",
            "Resource delivery is a week late. What you have in stock is now worth more.");
        Add("Coada de la ghișeu", "Очередь в окно", "Queue at the counter");
        Add("Pentru o simplă adeverință ai nevoie de alte trei adeverințe. Ziua pierdută costă.",
            "Для одной справки нужны ещё три справки. Потерянный день стоит денег.",
            "For one certificate you need three more. The lost day costs.");
    }
}
