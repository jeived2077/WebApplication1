Сама структура бд
CREATE DATABASE IF NOT EXISTS filmsdb;
USE filmsdb;

-- Создание таблицы Users
CREATE TABLE Users (
    IdUser INT PRIMARY KEY AUTO_INCREMENT,
    Login VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    Jwttoken TEXT
);

-- Создание таблицы Films
CREATE TABLE fims (
    Idfims INT PRIMARY KEY AUTO_INCREMENT,
    Namefilms VARCHAR(255),
    Image MEDIUMTEXT,
    Years VARCHAR(10),
    Information TEXT,
    Users INT,
    FOREIGN KEY (Users) REFERENCES Users(IdUser)
);

-- Создание таблицы Actor
CREATE TABLE actor (
    Idactor INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL
);

-- Создание таблицы Genre
CREATE TABLE genre (
    Idgenre INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL
);

-- Создание таблицы Director
CREATE TABLE director (
    Iddirector INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL
);

-- Создание таблицы FilmToActor (связь многие-ко-многим между фильмами и актерами)
CREATE TABLE filmstoactor (
    Idactor INT,
    Idfilms INT,
    PRIMARY KEY (Idactor, Idfilms),
    FOREIGN KEY (Idactor) REFERENCES actor(Idactor),
    FOREIGN KEY (Idfilms) REFERENCES fims(Idfims)
);

-- Создание таблицы FilmToDirector (связь многие-ко-многим между фильмами и режиссерами)
CREATE TABLE filmstodirector (
    Iddirector INT,
    Idfilms INT,
    PRIMARY KEY (Iddirector, Idfilms),
    FOREIGN KEY (Iddirector) REFERENCES director(Iddirector),
    FOREIGN KEY (Idfilms) REFERENCES fims(Idfims)
);

-- Создание таблицы FavoriteFilms
CREATE TABLE favoriteFilms (
    Ididfilms INT PRIMARY KEY AUTO_INCREMENT,
    IdUser INT,
    FOREIGN KEY (IdUser) REFERENCES Users(IdUser)
);

-- Создание таблицы ViewFilms
CREATE TABLE ViewFilms (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Films INT,
    FOREIGN KEY (Films) REFERENCES fims(Idfims)
);

-- Создание таблицы FilmToGenre (связь многие-ко-многим между фильмами и жанрами)
CREATE TABLE filmstogenre (
    Idfilms INT,
    Idgenre INT,
    PRIMARY KEY (Idfilms, Idgenre),
    FOREIGN KEY (Idfilms) REFERENCES fims(Idfims),
    FOREIGN KEY (Idgenre) REFERENCES genre(Idgenre)
);
