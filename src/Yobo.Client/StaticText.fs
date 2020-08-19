module Yobo.Client.StaticText

let terms = """
<div class="terms">
<h2>Podmínky rezervace:</h2>
<p>Lekce se konají na adrese: Centrum volného času GaPa, U pošty 822, Kostelec nad Labem.</p>
<p>
<ol>
<li>Pokud jdete na lekci poprvé, vyberte si lekci, na které je volno a klikněte na tlačítko Rezervovat. Cena první lekce tzv. na zkoušku je 50 Kč, částku uhradíte po skončení lekce.</li>
<li>Pokud chodíte nepravidelně nebo dáváte přednost platbě v hotovosti po každé lekci, klikněte na tlačítko Rezervovat. Cena druhé a dalších lekcí při platbě vždy po skončení lekce je 150 Kč. Při jednorázových platbách v hotovosti vždy lze dopředu zarezervovat pouze jednu lekci.</li>
<li>Cena permanentky 1200 Kč = 10 kreditů = 10 lekcí (cena jedné lekce 120 Kč), platnost 4 měsíce nebo 2200 Kč = 20 kreditů = 20 lekcí (cena jedné lekce 110 Kč), platnost 8 měsíců. Platnost všech permanentek je však maximálně do poslední lekce konané v aktuálním školním roce. K poslednímu červnu budou nevybrané kredity ze všech účtů smazány. Prosím, pohlídejte si, abyste všechny kredity stačili vychodit.</li>
<li>Permanentka je nepřenosná. S permanentkou lze navštěvovat jakékoliv lekce v Gapíku. Permanentku možno zakoupit buď platbou na účet 1681695016/3030 (do poznámky uveďte celé své jméno) nebo uhradit v hotovosti na lekci.</li>
<li>Po zakoupení permanentky Vám bude nahrán na účet příslušný počet kreditů. Máte možnost si zarezervovat dopředu tolik lekcí, kolik kreditů máte. Např. po nákupu jedné permanentky, můžete zarezervovat dopředu deset lekcí.</li>
<li>Přihlašování na lekce a odhlašování lze provádět kdykoliv s výjimkou dne konání lekce, kdy se od 10:00 lze pouze přihlašovat (viz. Storno podmínky).</li>
</ol>
</p>
<h2>Storno podmínky</h2>
<p>
<ol>
<li>Odhlášení z lekce je možno provést nejpozději v den konání lekce do 10:00 kliknutím na tlačítko Zrušit rezervaci.</li>
<li>V případě rezervace za kredit Vám bude kredit připsán zpět na účet. Pokud se nestihnete odhlásit do 10:00, kredit propadá.</li>
<li>V případě rezervace za hotovost bude přihlášení zrušeno a vy máte možnost přihlásit se na jinou lekci. Pokud se nestihnete odhlásit do 10:00, buď pošlete platbu za lekci na účet nebo platbu za lekci uhradíte při následující lekci. Pokud se podruhé přihlásíte na lekci, včas se neodhlásíte a nedorazíte, systém Vám neumožní další rezervaci lekce.</li>
<li>Přihlásit se na lekci, můžete kdykoliv před začátkem konání lekce, pokud je na ní volno. Pokud je tedy lekce obsazená, prosím sledujte rezervační systém, je možné, že se někdo odhlásil do 10:00.</li>
<li>Pokud jste si zakoupili permanentku a chcete chodit pouze na jednu lekci, může se stát, že lekce bude obsazena a vy si ji nebudete moci zarezervovat. Prosím sledujte rezervační systém a pokud se místo neuvolní v den lekce po 10:00, můžete mně kontaktovat a požádat o prodloužení platnosti permanentky. Platnost permanentky Vám bude o týden posunuta.</li>
<li>Pokud bude lekce zrušena, bude Vám připsán kredit. Obdržíte informační email.</li>
<li>Rezervací lekce souhlasím s podmínkami rezervace a storna lekcí.</li>
</ol>
</p>
<p>
Prosím o včasné odhlašovaní lekcí, umožníte tak všem, kdo mohou přijít, zúčastnit se lekce a Vám zbytečně nepropadne kredit nebo částka za lekci. Děkuji 😊
</p>
<h2>Ochrana osobních údajů</h2>
<p>Klient souhlasí se zpracováním a uchováním osobních údajů - jméno, příjmení a e-mailová adresa. Ochrana osobních údajů klienta, který je fyzickou osobou, je poskytována zákonem č. 101/2000 Sb., o ochraně osobních údajů, ve znění pozdějších předpisů. Klient souhlasí se zasíláním informací souvisejících se zbožím, službami nebo podnikem prodávajícího na e-mailovou adresu. Osobní údaje poskytnuté poskytovateli služeb přes internetové stránky nebudou bez souhlasu klienta předány třetí osobě.</p>
<p>Veškeré osobní údaje jako jméno, příjmení a emailová adresa jsou zašifrovány a uloženy v nečitelné podobě v cloudové službě Microsoft Azure Table Storage. V případě žádosti o zrušení účtu bude klíč k rozšifrování těchto údajů smazán.</p>
<p>V případě jakýchkoliv nejasností mne prosím neváhejte kontaktovat na tel. čísle 777 835 160 případně přes email provaznikova.cz@gmail.com.</p>
<p>
Ing. Jana Provazníková,</br>
IČO: 06680810</br>
Bankovní spojení: 1681695016/3030</br>
</p>
</div>
"""