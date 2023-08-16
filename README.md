# PizzaEcki
Das Projekt "PizzaEcki" ist eine Point-of-Sale-Anwendung für Pizzerien, die es ermöglicht, Bestellungen effizient zu verwalten und Kundeninformationen in einer SQLite-Datenbank zu speichern.

## Hauptfunktionen
- **Kundenverwaltung:** Kundeninformationen können gespeichert und bei Bedarf wieder abgerufen werden.
- **Bestellverwaltung:** Die Anwendung unterstützt die Eingabe von Gerichten, Extras und Bestellmengen.
- **Autocomplete-Funktion:** Die Eingabefelder bieten Autocomplete-Funktionen für effizientere Eingaben.

## Wichtige Klassen und Methoden

### DatabaseManager
Diese Klasse ist für die Verwaltung der Verbindung zur SQLite-Datenbank verantwortlich.

#### CreateTable()
Erstellt die benötigten Tabellen in der Datenbank, falls sie noch nicht existieren.

#### GetCustomerByPhoneNumber(string phoneNumber)
Holt Kundeninformationen anhand der Telefonnummer.

#### AddOrUpdateCustomer(Customer customer)
Fügt einen neuen Kunden hinzu oder aktualisiert einen bestehenden Kunden in der Datenbank.

### MainWindow
Die Hauptfensterklasse enthält Logik für die Benutzeroberfläche und Interaktion mit der Datenbank.

#### PhoneNumberTextBox_KeyDown(object sender, KeyEventArgs e)
Behandelt die Eingabe der Telefonnummer und sucht nach einem entsprechenden Kunden.

#### DishComboBox_TextChanged(object sender, SelectionChangedEventArgs e)
Verwaltet die Auswahl von Gerichten und Größen in den entsprechenden ComboBoxen.

## Installation und Verwendung
Klonen Sie das Repository und öffnen Sie das Projekt in einem kompatiblen Entwicklungsumgebung wie Visual Studio. Stellen Sie sicher, dass alle erforderlichen NuGet-Pakete installiert sind, bevor Sie die Anwendung ausführen.

## Lizenz
Dieses Projekt steht unter der MIT-Lizenz. Weitere Informationen finden Sie in der LICENSE-Datei.
