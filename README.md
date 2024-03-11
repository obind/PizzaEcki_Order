# PizzaEcki
Das Projekt "PizzaEcki" ist eine Point-of-Sale-Anwendung für Pizzerien, die es ermöglicht, Bestellungen effizient zu verwalten und Kundeninformationen in einer SQLite-Datenbank zu speichern.

## Hauptfunktionen
- **Kundenverwaltung:** Kundeninformationen können gespeichert und bei Bedarf wieder abgerufen werden.
- **Bestellverwaltung:** Die Anwendung unterstützt die Eingabe von Gerichten, Extras und Bestellmengen.
- **Autocomplete-Funktion:** Die Eingabefelder bieten Autocomplete-Funktionen für effizientere Eingaben.

## Installation und Verwendung
Klonen Sie das Repository und öffnen Sie das Projekt in einem kompatiblen Entwicklungsumgebung wie Visual Studio. Stellen Sie sicher, dass alle erforderlichen NuGet-Pakete installiert sind, bevor Sie die Anwendung ausführen.

## Lizenz
Dieses Projekt steht unter der MIT-Lizenz. Weitere Informationen finden Sie in der LICENSE-Datei.


`  `**PizzaEcki Datenbank Dokumentation Übersicht** 

Diese Dokumentation beschreibt die **DatabaseManager** Klasse im **PizzaEcki.Database** Namensraum. Die Hauptverantwortung dieser Klasse ist die Verwaltung der Datenbankoperationen für die PizzaEcki Anwendung, einschließlich der Initialisierung der Datenbank, der Erstellung von Tabellen und der Bereitstellung von Datenzugriffsmethoden.

**Klasse: DatabaseManager Konstruktor** 

- **Aufgabe:** Initialisiert eine neue Instanz des **DatabaseManager**, stellt eine Verbindung zur SQLite-Datenbank her, erstellt den Datenbankordner und die Datei (falls nicht vorhanden), und ruft die Methoden **CreateTable()** und **InitializeDishes()** auf. 
- **Pfad zur Datenbank:** Der Pfad setzt sich zusammen aus dem Benutzerdokumente - Ordner, dem Ordner **PizzaEckiDb** und der Datei **database.sqlite**. 

**Methoden** CreateTable() 

- **Aufgabe:** Erstellt die notwendigen Tabellen in der Datenbank, falls diese noch nicht existieren. Die Tabellen umfassen **Customers**, **Addresses**, **Gerichte**, **Extras**, **Drivers**, **Settings**, **OrderAssignments**, **Orders**, und **OrderItems**. 
- **Details:** 
  - **Customers** und **Addresses** sind durch Fremdschlüssel verbunden.
  - **OrderAssignments** verbindet Bestellungen (**Orders**) mit Fahrern (**Drivers**). 
  - **OrderItems** speichert die Details zu den einzelnen Bestellpositionen.

GetTableNames() 

- **Rückgabe:** Eine Liste der Namen aller Tabellen in der Datenbank.
- **Aufgabe:** Ruft die Namen aller Tabellen aus der **sqlite\_master** Tabelle ab. 

GetTableData(tableName) 

- **Parameter:** **tableName** (Name der Tabelle, deren Daten abgerufen werden sollen).
- **Rückgabe:** Eine **DataTable** mit den Daten der angeforderten Tabelle.
- **Aufgabe:** Führt eine **SELECT**-Abfrage aus, um alle Daten aus der angegebenen Tabelle zu laden. 

**GetCustomerByPhoneNumber(phoneNumber)**

- **Parameter:** **phoneNumber** (Telefonnummer des Kunden). 
- **Rückgabe:** Ein **Customer** Objekt mit den Daten des Kunden, oder **null**, falls kein Kunde mit der angegebenen Telefonnummer gefunden wird.
- **Aufgabe:** Sucht in der Datenbank nach einem Kunden mit der angegebenen Telefonnummer und gibt die Kundendaten zurück.

**UpdateCustomerData(customer)**Es wurden keine Einträge für das Inhaltsverzeichnis gefunden.** 

- **Parameter:** **customer** (Ein **Customer** Objekt mit den aktualisierten Kundendaten).
- **Aufgabe:** Aktualisiert die Daten eines bestehenden Kunden in der Datenbank.

**AddOrUpdateCustomer(customer)** 

- **Parameter:** **customer** (Ein **Customer** Objekt, das in der Datenbank hinzugefügt oder aktualisiert werden soll). 
- **Aufgabe:** Fügt einen neuen Kunden zur Datenbank hinzu oder aktualisiert die Daten eines bestehenden Kunden. Dabei wird auch die Adresse des Kunden in der **Addresses** Tabelle verwaltet. 

**AddDishes(dishes)** 

- **Parameter:** **dishes** (Eine Liste von **Dish** Objekten, die der Datenbank hinzugefügt werden sollen). 
- **Aufgabe:** Fügt mehrere Gerichte zur Datenbank hinzu oder aktualisiert sie.

**AddOrUpdateDish(dish)** 

- **Parameter:** **dish** (Ein **Dish** Objekt, das in der Datenbank hinzugefügt oder aktualisiert werden soll). 
- **Aufgabe:** Fügt ein neues Gericht zur Datenbank hinzu oder aktualisiert die Daten eines bestehenden Gerichts.

**GetAllDishes()** 

- **Rückgabe:** Eine Liste aller Gerichte in der Datenbank. 
- **Aufgabe:** Ruft alle Gerichte aus der Datenbank ab.

**IsIdExists(Id)** 

- **Parameter:** **Id** (Die ID des Gerichts). 
- **Rückgabe:** **true**, wenn ein Gericht mit der angegebenen ID existiert; sonst **false**. 
- **Aufgabe:** Überprüft, ob ein Gericht mit der angegebenen ID in der Datenbank existiert.

**DeleteDish(id)** 

- **Parameter:** **id** (Die ID des zu löschenden Gerichts). 
- **Aufgabe:** Löscht das Gericht mit der angegebenen ID aus der Datenbank.

**Hinweise zur Datenmodellierung** 

- Die **Customers** und **Addresses** Tabellen sind über den Fremdschlüssel **AddressId** miteinander verbunden. Dies ermöglicht es, Kundenadressen effizient zu verwalten.
- Gerichte (**Dishes**) können mit verschiedenen Attributen wie Preis, Kategorie und optionalen Beilagen gespeichert werden. Dies bietet Flexibilität bei der Menügestaltung.

**AddExtras(extras)** 

- **Parameter:** **extras** (Eine Liste von **Extra** Objekten, die der Datenbank hinzugefügt werden sollen). 
- **Aufgabe:** Fügt mehrere Zusatzartikel zur Datenbank hinzu oder aktualisiert sie.

**AddOrUpdateExtra(extra)** 

- **Parameter:** **extra** (Ein **Extra** Objekt, das in der Datenbank hinzugefügt oder aktualisiert werden soll). 
- **Aufgabe:** Fügt einen neuen Zusatzartikel zur Datenbank hinzu oder aktualisiert die Daten eines bestehenden Zusatzartikels.

**GetExtras()** 

- **Rückgabe:** Eine Liste aller Zusatzartikel in der Datenbank. 
- **Aufgabe:** Ruft alle Zusatzartikel aus der Datenbank ab.

**AddDriver(driver)** 

- **Parameter:** **driver** (Ein **Driver** Objekt, das der Datenbank hinzugefügt werden soll).
- **Aufgabe:** Fügt einen neuen Fahrer zur Datenbank hinzu.

**UpdateDriver(driver)** 

- **Parameter:** **driver** (Ein **Driver** Objekt mit aktualisierten Daten). 
- **Aufgabe:** Aktualisiert die Daten eines bestehenden Fahrers in der Datenbank.

**InitializeStaticDrivers()** 

- **Aufgabe:** Fügt statische Fahrereinträge zur Datenbank hinzu, die für interne Zwecke genutzt werden können. 

**DeleteDriver(id)** 

- **Parameter:** **id** (Die ID des zu löschenden Fahrers). 
- **Aufgabe:** Löscht den Fahrer mit der angegebenen ID aus der Datenbank.

**UnassignDriverFromOrders(driverId)** 

- **Parameter:** **driverId** (Die ID des Fahrers, der von den Bestellungen abgezogen werden soll). 
- **Aufgabe:** Entfernt die Zuweisung eines Fahrers von allen Bestellungen, indem der Fahrer in den Bestellungen auf einen Standardwert gesetzt wird.

**GetDrivers() & GetAllDrivers()** 

- Beide Methoden holen alle Fahrer aus der Datenbank, aber es scheint, dass **GetAllDrivers()** eine duplizierte Funktionalität von **GetDrivers()** darstellt. Überprüfen und konsolidieren Sie die Methoden bei Bedarf.

**GetCurrentBonNumber()** 

- **Rückgabe:** Die aktuelle Bonnummer aus den Einstellungen.
- **Aufgabe:** Ruft die aktuelle Bonnummer aus der **Settings** Tabelle ab. 

**DeleteOrderAsync(orderId)** 

- **Parameter:** **orderId** (Die ID der zu löschenden Bestellung). 
- **Rückgabe:** Ein **bool**, das angibt, ob das Löschen erfolgreich war.
- **Aufgabe:** Löscht eine Bestellung und alle zugehörigen Daten (Bestellpositionen und Zuweisungen) asynchron aus der Datenbank.

**SaveOrderAssignment(orderId, driverId, price)**  

- **Parameter:** **orderId** (ID der Bestellung), **driverId** (ID des Fahrers), **price** (Preis der Bestellung). 
- **Aufgabe:** Speichert oder aktualisiert eine Zuordnung zwischen einer Bestellung und einem Fahrer in der Datenbank. Wenn bereits eine Zuordnung für die gegebene **orderId** existiert, wird diese aktualisiert, sonst wird eine neue Zuordnung erstellt.

**SaveOrder(order)**  

- **Parameter:** **order** (Ein **Order** Objekt, das gespeichert werden soll).
- **Aufgabe:** Speichert eine Bestellung und zugehörige Bestellpositionen (**OrderItems**) in 

  der Datenbank. Zudem wird ein Eintrag in der **OrderAssignments** Tabelle mit einer **NULL** **DriverId** erstellt, um die Bestellung als unzugewiesen zu markieren.

**GetUnassignedOrders()**  

- **Rückgabe:** Eine Liste von **Order** Objekten, die noch keinem Fahrer zugewiesen wurden.
- **Aufgabe:** Ruft alle Bestellungen aus der Datenbank ab, die aktuell keinem Fahrer zugewiesen sind. 

**GetAllOrders()****   

- **Rückgabe:** Eine Liste aller Bestellungen in der Datenbank.
- **Aufgabe:** Ruft alle Bestellungen und zugehörigen Bestellpositionen aus der Datenbank ab. 

**DeleteDailyOrdersAsync()**  

- **Aufgabe:** Löscht asynchron alle Bestellungen, Bestellzuordnungen (**OrderAssignments**), und Bestellpositionen (**OrderItems**) aus der Datenbank. Diese Methode könnte für das Zurücksetzen der Datenbank am Ende eines Geschäftstages verwendet werden. 

**GetOrderItems(orderId)****   

- **Parameter:** **orderId** (Die ID der Bestellung, für die Bestellpositionen abgerufen werden sollen). 
- **Rückgabe:** Eine Liste von **OrderItem** Objekten, die zu einer spezifischen Bestellung gehören. 
- **Aufgabe:** Ruft alle Bestellpositionen für eine gegebene Bestell-ID aus der Datenbank ab. 

**GetOrderAssignments()****   

- **Rückgabe:** Eine Liste von **OrderAssignment** Objekten, die Zuordnungen von Bestellungen zu Fahrern repräsentieren.
- **Aufgabe:** Ruft alle Bestellzuordnungen aus der Datenbank ab.

**GetTotalSalesForDate(date)****   

- **Parameter:** **date** (Das Datum, für das der Gesamtumsatz abgerufen werden soll).
- **Rückgabe:** Die Summe aller Verkäufe (Preis) für ein spezifisches Datum.
- **Aufgabe:** Berechnet den Gesamtumsatz für ein gegebenes Datum basierend auf den Daten in der **OrderAssignments** Tabelle. 

**GetDailySales(date)**  

- **Parameter:** **date** (Das Datum, für das die täglichen Umsätze abgerufen werden sollen).
- **Rückgabe:** Eine Liste von **DailySalesInfo** Objekten, die die täglichen Umsätze nach Fahrern oder Abholtheke zusammenfassen.
- **Aufgabe:** Ruft den täglichen Umsatz für ein bestimmtes Datum aus der Datenbank ab und gruppiert diesen nach Fahrern oder der Abholtheke.

**CheckAndResetBonNumberIfNecessary()**  

- **Rückgabe:** Die aktuelle Bonnummer, entweder zurückgesetzt oder unverändert.
- **Aufgabe:** Überprüft, ob das Datum des letzten Zurücksetzens der Bonnummer in der Vergangenheit liegt und setzt die Bonnummer zurück, falls notwendig. Diese Methode sorgt dafür, dass die Bonnummern zu Beginn eines neuen Tages oder eines neuen Zeitraums zurückgesetzt werden. 

**ResetBonNumberForTesting()**  

- **Aufgabe:** Setzt die Bonnummer zu Testzwecken zurück.
- **Details:** Aktualisiert die **Settings** Tabelle, indem die **CurrentBonNumber** auf 1 gesetzt und das **LastResetDate** auf das aktuelle Datum aktualisiert wird. Diese Methode ist besonders nützlich in Testumgebungen, um die Funktionalität rund um die Bonnummerierung ohne Beeinträchtigung der Produktionsdaten zu überprüfen.

**UpdateCurrentBonNumber(newNumber)**  

- **Parameter:** **newNumber** (Die neue Bonnummer, die gesetzt werden soll).
- **Aufgabe:** Aktualisiert die aktuelle Bonnummer in der **Settings** Tabelle mit einem neuen Wert. Diese Methode ermöglicht eine flexible Anpassung der Bonnummer, was in bestimmten Szenarien wie beispielsweise der Korrektur von Fehlern oder der Anpassung nach bestimmten Geschäftsereignissen erforderlich sein kann.

**Dispose()**  

- **Aufgabe:** Schließt und entsorgt die Datenbankverbindung.
- **Details:** Diese Methode sorgt für eine ordnungsgemäße Freigabe der Ressourcen, die von der **SqliteConnection** Instanz verwendet werden. Durch den Aufruf von **Dispose** wird sichergestellt, dass die Verbindung zur Datenbank korrekt geschlossen und der damit verbundene Speicher freigegeben wird. Dies ist besonders wichtig, um Speicherlecks und andere Ressourcenmanagementprobleme zu vermeiden.

Mit diesen Ergänzungen ist die Dokumentation der **DatabaseManager** Klasse nun komplett. Die beschriebenen Methoden decken ein breites Spektrum an Funktionalitäten ab, von der Verwaltung von Bestellungen und Fahrern bis hin zur Verarbeitung von Tagesabschlüssen und der Handhabung von Bonnummern. Die Verwendung dieser Methoden ermöglicht eine effiziente Steuerung und Überwachung der Kerngeschäftsprozesse innerhalb der Anwendung.
