# Program do grafikowania personelu medycznego

## Opis projektu
Glównym celem projektu jest stworzenie systemu wspomagającego tworzenie grafiku dla peronelu medycznego. 

### Bieżąca lista ToDo
1. Poprawić generowanie raportów: 
    - Wprowadzić dynamiczne generowanie raportów dla każedej grupy pracowników oddziału,
    - wprowadzić generowanie *.HTML -> *.pdf.
3. Określić konwencję dotyczącą wprowadzania poziomu ważnośći próśb - bardzo ważne - mało ważne,
5. Zebrać wszelkie informacje prawne, dotyczące harmonogramowania pracy.

### Mile stones

1. Stworzenie zamysłu projektu ✔️
2. Dobór odpowiednich narzędzi ✔️
3. Szkic aplikacji ✔️
4. Projekt interfejsu oraz testy


### Tutoriale z których Korzystam:

1. <a href="https://www.youtube.com/watch?v=gSfMNjWNoX0&list=PLLWMQd6PeGY3QEHCmCWaUKNhmFFdIDxE8">WPF with Tim Corey</a>
2. <a href="https://www.youtube.com/watch?v=66K5Nmmc9g8">SQLite</a>
3. <a href="https://www.youtube.com/watch?v=ayp3tHEkRc0">How to create DB file</a>
4. <a href="https://www.udemy.com/course/windows-presentation-foundation-masterclass/">Windows Presentation Foundation Masterclass</a>


## Szkic projektu

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209861866-0951b65c-996a-4c1b-92e6-fba874d71cbb.png">
</p>

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209861870-14fe2077-65a8-45d3-a04b-1c629e0cc620.png">
</p>

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209861874-a624cd19-85b8-4435-9d48-a560e6342445.png">
</p>

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209862013-c71dafcf-9c33-496a-8fce-4d656c949040.png">
</p>

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209864580-3383a085-7335-4693-bf66-fb68a7e4a92a.png">
</p>

<p align="center">
    <img width="1000" src="https://user-images.githubusercontent.com/96399051/209864283-d164a71d-be58-4da8-9dc6-d60022bc5143.png">
</p>

## Schemat działania podczas układania grafiku:

UŁÓŻ GRAFIK(przycisk w menu głównym)      
&emsp;&emsp;&emsp;| --> WPROWADŹ PROŚBY(pierwszy z etapów układania grafiku)  
&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;| --> sprawdź, czy na ten miesiąc nie zostały wprowadzone już prośby  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;| --> zapytaj użytkownika, czy chce wprowadzić nowe prośby, czy modifikować istniejące, postępuj zgodnie z wyborem  
&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|--> UŁÓŻ GRAFIK (przycisk do układania grafiku na podstawie próśb)    
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;| --> UŁÓŻ GRAFIK NA PODSTAWIE KODEKSU PRACY ORAZ OBOWIĄZUJĄCEGO PRAWA W SZPITALU  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|--> Popraw grafik tak, by spełnić jak najwięcej próśb pracowników  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;| --> Wyświetl gotowy grafik, w okienku zawierającym LOGI przedstaw niespełnione prośby  
&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|--> ZAPISZ GRAFIK (w odpowiednim folderze)  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|  
&emsp;&emsp;&emsp;|&emsp;&emsp;&emsp;|--> Użytkownik wprowadza nazwę


