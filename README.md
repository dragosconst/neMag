# neMag
neMag is a shopping website written in ASP.NET MVC5, which offers a wide range of features that enhance usability, providing a pleasant and unique shopping experience. Users can post reviews, ask questions about any products and mark their favorite products, for better recommendations. The website offers a customizable user panel.<br/><br/>
Collaborating shops can sell their products on the website, given they pass our quality checks made by our administrators. We provide them with various statistics about their products, so can they improve their selling offers.<br/><br/>
The website has a modern design, made with ease-of-use in mind. User navigation is facilitated by our user-friendly interface and the robust search mechanism. 

## UML states diagram
![neMag_website_State_Diagram](https://user-images.githubusercontent.com/38582034/121907613-e346e600-cd34-11eb-88c2-4198b81a7a45.png)


## Build tool  
We used the default Visual Studio build tool, MSBuild. It is integrated by default with Visual Studio for ASP.NET applications, it works using a special .csproj file, where it automatically links all dependencies. We used NuGet as our packet manager.

## User stories
We compiled a list of user stories, they can also be found on our Jira website. Due to the fact that most of our userbase is Romanian, the user stories are also written in Romanian:
1. Eu ca utilizator logat, doresc sa pot adresa intrebari comunitatii, pentru a ma informa mai bine despre produse.
2. Eu ca utilizator nelogat, doresc să pot vizualiza produse în functie de categorie pentru a economisi timp.
3. Eu ca patron, doresc ca numai utilizatorii înregistrați să poată să efectueze comenzi, pentru a avea o evidență clară a consumatorilor.
4. Eu ca colaborator, doresc sa am o pagina publica, pentru a-mi prezenta produsele.
5. Eu ca utilizator logat, doresc sa am o pagina de profil, pentru a tine evidenta cumparaturilor.
6. Eu ca patron, doresc să-mi rezerv dreptul de a nu accepta anumite produse, pentru a selecta numai produsele de calitate superioară.
7. Eu ca utilizator logat, doresc să pot comanda mai multe produse deodată, pentru a diminua costul de transport.
8. Eu ca colaborator, doresc să pot observa statistici despre produsele pe care le-am adăugat, pentru a-mi îmbunătăți oferta.
9. Eu ca colaborator, doresc să adaug un produs cât mai ușor pentru a economisi timp.
10. Eu ca utilizator nelogat, doresc să pot vizualiza cele mai ieftine produse pentru a economisi bani.
11. Eu ca utilizator nelogat, doresc să pot vizualiza recenziile altor cumpărători pentru a face o alegere mai informată.
12. Eu ca utilizator nelogat, doresc sa am o interfata atractiva si usor de folosit, pentru a-mi imbunatati experienta de navigare.


## Unit testing
We wrote 20 unit tests, that cover a range of features present in our application. We used the default framework for Visual Basic, which is MSTest. Due to the fact that our application is heavily dependent on a database connection, we used a mocking library, Moq, to write almost all the tests. <br/><br/>
The tests are written following the usual conventions for unit testing.
