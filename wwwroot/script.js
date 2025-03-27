const BASE_URL = "http://localhost:5184";
let token = localStorage.getItem("jwtToken");

const userInfo = document.getElementById("user-info");
const userLoginSpan = document.getElementById("user-login");
const authButton = document.getElementById("auth-button");
const authModal = document.getElementById("auth-modal");
const movieModal = document.getElementById("movie-modal");
const closeModalButtons = document.getElementsByClassName("close");
const authForm = document.getElementById("auth-form");
const modalTitle = document.getElementById("modal-title");
const submitButton = document.getElementById("submit-button");
const authError = document.getElementById("auth-error");
const switchAuth = document.getElementById("switch-auth");
const adminActions = document.getElementById("admin-actions");
const userActions = document.getElementById("user-actions");
const movieGrid = document.querySelector(".movie-grid");
const noMoviesMessage = document.getElementById("no-movies-message");
const addMovieButton = document.getElementById("add-movie");
const addMovieUserButton = document.getElementById("add-movie-user");
const movieForm = document.getElementById("movie-form");

const genreSelect = document.getElementById("genre");
const directorSelect = document.getElementById("director");
const sortSelect = document.getElementById("sort");
const applyFiltersButton = document.getElementById("applyFilters");

const saveButton = document.getElementById("save-button");
const updateButton = document.getElementById("update-button");
const deleteButton = document.getElementById("delete-button");
const movieTableBody = document.getElementById("movie-table-body");

let isLoginMode = true;
let allMovies = [];
let allGenres = [];
let allDirectors = [];
let allActors = [];
let selectedMovieId = null;
let selectedPoster = null;

function checkAuth() {
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const login = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload["name"];
            const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
            const userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
            if (!login) throw new Error("Логин не найден в токене");

            userLoginSpan.textContent = login;
            userInfo.style.display = "inline";
            authButton.textContent = "Выйти";
            authButton.onclick = logout;

            if (role === "admin") {
                adminActions.style.display = "block";
                userActions.style.display = "none";
            } else if (role === "user") {
                adminActions.style.display = "none";
                userActions.style.display = "block";
            }

            localStorage.setItem("userId", userId);
        } catch (e) {
            console.error("Ошибка декодирования токена:", e);
            logout();
        }
    } else {
        userInfo.style.display = "none";
        authButton.textContent = "Войти";
        authButton.onclick = showAuthModal;
        adminActions.style.display = "none";
        userActions.style.display = "none";
    }
}

function showAuthModal() {
    authModal.style.display = "block";
    modalTitle.textContent = "Авторизация";
    submitButton.textContent = "Войти";
    isLoginMode = true;
    authError.textContent = "";
}

for (let closeButton of closeModalButtons) {
    closeButton.onclick = function () {
        const modal = this.closest(".modal");
        if (modal) modal.style.display = "none";
    };
}

switchAuth.onclick = function (e) {
    e.preventDefault();
    isLoginMode = !isLoginMode;
    modalTitle.textContent = isLoginMode ? "Авторизация" : "Регистрация";
    submitButton.textContent = isLoginMode ? "Войти" : "Зарегистрироваться";
    switchAuth.textContent = isLoginMode ? "Перейти к регистрации" : "Перейти к авторизации";
    authError.textContent = "";
};

function logout() {
    localStorage.removeItem("jwtToken");
    localStorage.removeItem("userId");
    token = null;
    checkAuth();
    loadMovies();
}

authForm.onsubmit = async function (e) {
    e.preventDefault();
    const login = document.getElementById("login").value;
    const password = document.getElementById("password").value;

    try {
        const url = isLoginMode ? `${BASE_URL}/api/auth/login` : `${BASE_URL}/api/auth/register`;
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ login, password })
        });

        const text = await response.text();
        if (response.ok) {
            token = text;
            localStorage.setItem("jwtToken", token);
            authModal.style.display = "none";
            checkAuth();
            loadMovies();
        } else {
            authError.textContent = text;
        }
    } catch (error) {
        authError.textContent = `Ошибка запроса: ${error.message}`;
    }
};

async function loadMovies() {
    try {
        const response = await fetch(`${BASE_URL}/api/movies`);
        if (!response.ok) throw new Error(`Ошибка загрузки данных: ${response.statusText}`);
        const data = await response.json();

        allMovies = data.movies || [];
        allGenres = data.genres || [];
        allDirectors = data.directors || [];
        allActors = data.actors || [];

        genreSelect.innerHTML = allGenres.length === 0
            ? '<option value="">Нету данных</option>'
            : '<option value="">Все</option>' + allGenres.map(g => `<option value="${g}">${g}</option>`).join('');

        directorSelect.innerHTML = allDirectors.length === 0
            ? '<option value="">Нету данных</option>'
            : '<option value="">Все</option>' + allDirectors.map(d => `<option value="${d}">${d}</option>`).join('');

        applyFilters();
        populateMovieTable();
    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
        movieGrid.innerHTML = "";
        noMoviesMessage.style.display = "block";
        noMoviesMessage.textContent = "Ошибка загрузки данных";
        genreSelect.innerHTML = '<option value="">Нету данных</option>';
        directorSelect.innerHTML = '<option value="">Нету данных</option>';
    }
}
const uploadPosterButton = document.getElementById("upload-poster-btn");
const posterInput = document.getElementById("poster");
const posterStatus = document.getElementById("poster-status");

// Переменная для хранения загруженного постера (base64)
let uploadedPoster = null;

// Открытие проводника при нажатии на кнопку
uploadPosterButton.onclick = function () {
    posterInput.click(); // Программно вызываем клик на скрытый input
};
posterInput.addEventListener("change", async function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = async function (event) {
            const base64Image = event.target.result; // Получаем base64 изображения
            try {
                posterStatus.textContent = "Загрузка...";
                uploadedPoster = base64Image; // Сохраняем base64 в переменную
                posterStatus.textContent = "Постер успешно загружен!";
                posterStatus.style.color = "green";
            } catch (error) {
                console.error("Ошибка при загрузке постера:", error);
                posterStatus.textContent = "Ошибка при загрузке постера";
                posterStatus.style.color = "red";
            }
        };
        reader.readAsDataURL(file); // Читаем файл как Data URL (base64)
    }
});
function applyFilters() {
    const genreFilter = genreSelect.value;
    const directorFilter = directorSelect.value;
    const sortOrder = sortSelect.value;

    let filteredMovies = [...allMovies];
    if (genreFilter && genreFilter !== "Нету данных") {
        filteredMovies = filteredMovies.filter(movie => movie.genre === genreFilter);
    }
    if (directorFilter && directorFilter !== "Нету данных") {
        filteredMovies = filteredMovies.filter(movie => movie.directors.split(", ").includes(directorFilter));
    }

    if (sortOrder === "asc") {
        filteredMovies.sort((a, b) => parseInt(a.year) - parseInt(b.year));
    } else if (sortOrder === "desc") {
        filteredMovies.sort((a, b) => parseInt(b.year) - parseInt(a.year));
    }

    movieGrid.innerHTML = "";
    if (filteredMovies.length === 0) {
        noMoviesMessage.style.display = "block";
        noMoviesMessage.textContent = "Фильмы не найдены.";
        return;
    }

    noMoviesMessage.style.display = "none";
    filteredMovies.forEach(movie => {
        const movieCard = document.createElement("div");
        movieCard.classList.add("movie-card");
        movieCard.innerHTML = `
            <h3>${movie.title}</h3>
            <p>Год: ${movie.year}</p>
            <p>Жанр: ${movie.genre}</p>
            <p>Режиссеры: ${movie.directors || "Не указано"}</p>
            <p>Актеры: ${movie.actors || "Не указано"}</p>
            ${movie.poster ? `<img src="${movie.poster}" alt="${movie.title}" style="max-width: 100%; height: auto;">` : ""}
        `;
        movieGrid.appendChild(movieCard);
    });
}

function showMovieModal() {
    movieModal.style.display = "block";
    resetForm();
    populateMovieTable();
}

function resetForm() {
    selectedMovieId = null;
    uploadedPoster = null;

    const movieIdInput = document.getElementById("movie-id");
    const titleInput = document.getElementById("title");
    const yearInput = document.getElementById("year");
    const descriptionTextarea = document.getElementById("description");
    const genreModalSelect = document.getElementById("genre-modal");
    const directorModalSelect = document.getElementById("director-modal");
    const actorsModalSelect = document.getElementById("actors-modal");

    if (movieIdInput) movieIdInput.value = "";
    if (titleInput) titleInput.value = "";
    if (yearInput) yearInput.value = "";
    if (descriptionTextarea) descriptionTextarea.value = "";
    if (posterInput) posterInput.value = ""; // Убедитесь, что posterInput определён
    if (posterStatus) posterStatus.textContent = ""; // Убедитесь, что posterStatus определён

    saveButton.style.display = "inline";
    updateButton.style.display = "none";
    deleteButton.style.display = "none";

    // Заполнение жанров
    if (genreModalSelect) {
        genreModalSelect.innerHTML = '<option value="">Выберите жанр</option>' +
            (allGenres.length === 0
                ? '<option value="">Нету данных</option>'
                : allGenres.map(genre => `<option value="${genre}">${genre}</option>`).join('')) +
            '<option value="add-new" class="add-new-option">Добавить новый жанр...</option>';
    }

    // Заполнение режиссёров
    if (directorModalSelect) {
        directorModalSelect.innerHTML = '<option value="">Выберите режиссера</option>' +
            (allDirectors.length === 0
                ? '<option value="">Нету данных</option>'
                : allDirectors.map(director => `<option value="${director}">${director}</option>`).join('')) +
            '<option value="add-new" class="add-new-option">Добавить нового режиссера...</option>';
    }

    // Заполнение актёров
    if (actorsModalSelect) {
        actorsModalSelect.innerHTML = '<option value="">Выберите актера</option>' +
            (allActors.length === 0
                ? '<option value="">Нету данных</option>'
                : allActors.map(actor => `<option value="${actor}">${actor}</option>`).join('')) +
            '<option value="add-new" class="add-new-option">Добавить нового актера...</option>';
    }
}

document.getElementById("director-modal").addEventListener("change", async function (e) {
    if (e.target.value === "add-new") {
        const firstName = prompt("Введите имя режиссера:");
        const lastName = prompt("Введите фамилию режиссера:");
        if (firstName && lastName) {
            const newDirector = `${firstName} ${lastName}`;
            try {
                const response = await fetch(`${BASE_URL}/api/directors`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({ firstName, lastName })
                });
                if (response.ok) {
                    await loadMovies(); // Обновляем все данные
                    const directorModalSelect = document.getElementById("director-modal");
                    directorModalSelect.innerHTML = '<option value="">Выберите режиссера</option>' +
                        allDirectors.map(director => `<option value="${director}">${director}</option>`).join('') +
                        '<option value="add-new" class="add-new-option">Добавить нового режиссера...</option>';
                    Array.from(directorModalSelect.options).forEach(opt => {
                        if (opt.value === newDirector) opt.selected = true;
                    });
                } else {
                    alert("Ошибка при добавлении режиссера");
                    resetForm();
                }
            } catch (error) {
                console.error("Ошибка:", error);
                alert("Ошибка при добавлении режиссера");
                resetForm();
            }
        } else {
            resetForm();
        }
    }
});

document.getElementById("actors-modal").addEventListener("change", async function (e) {
    const selectedOption = e.target.options[e.target.selectedIndex];
    if (selectedOption && selectedOption.value === "add-new") {
        const firstName = prompt("Введите имя актера:");
        const lastName = prompt("Введите фамилию актера:");
        if (firstName && lastName) {
            const newActor = `${firstName} ${lastName}`;
            try {
                const response = await fetch(`${BASE_URL}/api/actors`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({ firstName, lastName })
                });
                if (response.ok) {
                    await loadMovies(); // Обновляем все данные
                    const actorsModalSelect = document.getElementById("actors-modal");
                    actorsModalSelect.innerHTML = '<option value="">Выберите актера</option>' +
                        allActors.map(actor => `<option value="${actor}">${actor}</option>`).join('') +
                        '<option value="add-new" class="add-new-option">Добавить нового актера...</option>';
                    Array.from(actorsModalSelect.options).forEach(opt => {
                        if (opt.value === newActor) opt.selected = true;
                    });
                } else {
                    alert("Ошибка при добавлении актера");
                    resetForm();
                }
            } catch (error) {
                console.error("Ошибка:", error);
                alert("Ошибка при добавлении актера");
                resetForm();
            }
        } else {
            resetForm();
        }
    }
});

document.getElementById("poster").addEventListener("change", async function (e) {
    const file = e.target.files[0];
    if (file) {
        const formData = new FormData();
        formData.append("poster", file);
        try {
            const response = await fetch(`${BASE_URL}/api/upload-poster`, {
                method: "POST",
                headers: { "Authorization": `Bearer ${token}` },
                body: formData
            });
            const data = await response.json();
            if (response.ok) {
                selectedPoster = data.poster;
            } else {
                alert("Ошибка при загрузке постера");
            }
        } catch (error) {
            console.error("Ошибка при загрузке постера:", error);
            alert("Ошибка при загрузке постера");
        }
    }
});


function populateMovieTable() {
    movieTableBody.innerHTML = "";
    if (allMovies.length === 0) {
        movieTableBody.innerHTML = '<tr><td colspan="8">Фильмы не найдены</td></tr>';
        return;
    }

    allMovies.forEach(movie => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${movie.title || "Без названия"}</td>
            <td>${movie.genre || "Нет жанра"}</td>
            <td>${movie.directors || "Нет режиссеров"}</td>
            <td>${movie.year || "Нет года"}</td>
            <td>${movie.actors || "Нет актеров"}</td>
            <td>${movie.poster ? '<img src="' + movie.poster + '" style="max-width: 50px;">' : "Нет"}</td>
            <td>${movie.description || "Нет описания"}</td>
            <td>${movie.creator || "Неизвестно"}</td> <!-- Отображаем создателя -->
        `;
        row.style.cursor = "pointer";
        row.addEventListener("click", () => {
            editMovie(movie.id);
            movieModal.style.display = "block"; // Открываем модальное окно
        });
        movieTableBody.appendChild(row);
    });
}
function editMovie(movieId) {
    const movie = allMovies.find(m => m.id === movieId);
    if (movie) {
        selectedMovieId = movieId;
        uploadedPoster = movie.poster;
        document.getElementById("movie-id").value = movie.id;
        document.getElementById("title").value = movie.title;
        document.getElementById("genre-modal").value = movie.genre;
        const directorModalSelect = document.getElementById("director-modal");
        Array.from(directorModalSelect.options).forEach(opt => {
            opt.selected = movie.directors.split(", ").includes(opt.value);
        });
        document.getElementById("year").value = movie.year;
        document.getElementById("description").value = movie.description || "";
        const actorsModalSelect = document.getElementById("actors-modal");
        Array.from(actorsModalSelect.options).forEach(opt => {
            opt.selected = movie.actors.split(", ").includes(opt.value);
        });
        posterStatus.textContent = movie.poster ? "Текущий постер загружен" : "Постер не загружен";
        posterStatus.style.color = movie.poster ? "green" : "black";
        saveButton.style.display = "none"; // Скрываем "Сохранить"
        updateButton.style.display = "inline"; // Показываем "Обновить"
        deleteButton.style.display = "inline"; // Показываем "Удалить"
    }
}

addMovieButton.onclick = showMovieModal;
addMovieUserButton.onclick = showMovieModal;

movieForm.onsubmit = async function (e) {
    e.preventDefault();
    const title = document.getElementById("title").value;
    const genre = document.getElementById("genre-modal").value;
    const directors = Array.from(document.getElementById("director-modal").selectedOptions).map(opt => opt.value);
    const year = document.getElementById("year").value;
    const description = document.getElementById("description").value;
    const actors = Array.from(document.getElementById("actors-modal").selectedOptions).map(opt => opt.value);
    const poster = uploadedPoster; // Используем загруженный постер

    try {
        const response = await fetch(`${BASE_URL}/api/movies`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                title,
                description,
                poster, // Передаем base64 строку
                year,
                genre,
                directors,
                actors
            })
        });

        if (!response.ok) throw new Error(await response.text());

        movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при добавлении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

updateButton.onclick = async function () {
    if (!selectedMovieId) return;

    const title = document.getElementById("title").value;
    const genre = document.getElementById("genre-modal").value;
    const directors = Array.from(document.getElementById("director-modal").selectedOptions).map(opt => opt.value);
    const year = document.getElementById("year").value;
    const description = document.getElementById("description").value;
    const actors = Array.from(document.getElementById("actors-modal").selectedOptions).map(opt => opt.value);
    const poster = uploadedPoster || selectedPoster; // Используем новый постер или старый, если новый не загружен

    try {
        const response = await fetch(`${BASE_URL}/api/movies/${selectedMovieId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                title,
                description,
                poster, // Передаем base64 строку
                year,
                genre,
                directors,
                actors
            })
        });

        if (!response.ok) throw new Error(await response.text());

        movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при обновлении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

deleteButton.onclick = async function () {
    if (!selectedMovieId) return;

    if (!confirm("Вы уверены, что хотите удалить этот фильм?")) return;

    try {
        const response = await fetch(`${BASE_URL}/api/movies/${selectedMovieId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error(await response.text());

        movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при удалении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

async function deleteDirectorFromMovie(movieId, directorName) {
    try {
        const response = await fetch(`${BASE_URL}/api/movies/${movieId}/directors/${encodeURIComponent(directorName)}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.ok) {
            await loadMovies();
            editMovie(movieId);
        } else {
            alert("Ошибка при удалении режиссера");
        }
    } catch (error) {
        console.error("Ошибка при удалении режиссера:", error);
        alert("Ошибка при удалении режиссера");
    }
}

async function deleteActorFromMovie(movieId, actorName) {
    try {
        const response = await fetch(`${BASE_URL}/api/movies/${movieId}/actors/${encodeURIComponent(actorName)}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.ok) {
            await loadMovies();
            editMovie(movieId);
        } else {
            alert("Ошибка при удалении актера");
        }
    } catch (error) {
        console.error("Ошибка при удалении актера:", error);
        alert("Ошибка при удалении актера");
    }
}

window.onload = function () {
    checkAuth();
    loadMovies();
};