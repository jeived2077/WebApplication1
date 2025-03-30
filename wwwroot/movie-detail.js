const BASE_URL = "http://localhost:5184";

const urlParams = new URLSearchParams(window.location.search);
const movieId = urlParams.get("id");
const token = localStorage.getItem("jwtToken");

const elements = {
    moviePoster: document.getElementById("movie-poster"),
    movieTitle: document.getElementById("movie-title"),
    movieGenre: document.getElementById("movie-genre"),
    movieDirector: document.getElementById("movie-director"),
    movieActors: document.getElementById("movie-actors"),
    movieYear: document.getElementById("movie-year"),
    movieViews: document.getElementById("movie-views"),
    movieDescription: document.getElementById("movie-description"),
    movieCreator: document.getElementById("movie-creator")
    // editButton removed since it's not in the HTML
};

async function loadMovieDetails() {
    if (!movieId) {
        alert("ID фильма не указан!");
        window.location.href = "homepage.html";
        return;
    }

    try {
        const response = await fetch(`${BASE_URL}/api/movies/${movieId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token || ''}`
            }
        });

        if (!response.ok) {
            throw new Error(`Ошибка загрузки данных: ${response.statusText}`);
        }

        const movie = await response.json();

        // Заполняем элементы страницы
        elements.moviePoster.src = movie.poster || "placeholder.jpg";
        elements.moviePoster.alt = movie.title || "Без названия";
        elements.movieTitle.textContent = movie.title || "Без названия";
        elements.movieGenre.textContent = movie.genres || "Нет жанров";
        elements.movieDirector.textContent = movie.directors || "Нет режиссёров";
        elements.movieActors.textContent = movie.actors || "Нет актёров";
        elements.movieYear.textContent = movie.year || "Нет года";
        elements.movieDescription.textContent = movie.description || "Нет описания";
        elements.movieViews.textContent = movie.viewCount || 0;
        elements.movieCreator.textContent = movie.creator || "Неизвестно";

        // Обновляем счетчик просмотров
        await updateViewCount(movieId);
    } catch (error) {
        console.error("Ошибка при загрузке деталей фильма:", error);
        alert(`Ошибка: ${error.message}`);
        window.location.href = "homepage.html";
    }
}

async function updateViewCount(movieId) {
    try {
        await fetch(`${BASE_URL}/api/movies/${movieId}/view`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token || ''}`
            }
        });
    } catch (error) {
        console.error("Ошибка при обновлении счетчика просмотров:", error);
    }
}

window.onload = async function () {
    await loadMovieDetails();

    // Обработчик возврата на главную страницу
    document.querySelector("a[href='homepage.html']").addEventListener("click", (e) => {
        e.preventDefault();
        window.location.href = "homepage.html";
    });
};