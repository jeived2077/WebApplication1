const BASE_URL = "http://localhost:5184";
let token = localStorage.getItem("jwtToken");

const elements = {
    userInfo: document.getElementById("user-info"),
    userLoginSpan: document.getElementById("user-login"),
    authButton: document.getElementById("auth-button"),
    authModal: document.getElementById("auth-modal"),
    movieModal: document.getElementById("movie-modal"),
    closeModalButtons: document.getElementsByClassName("close"),
    authForm: document.getElementById("auth-form"),
    modalTitle: document.getElementById("modal-title"),
    submitButton: document.getElementById("submit-button"),
    authError: document.getElementById("auth-error"),
    switchAuth: document.getElementById("switch-auth"),
    adminActions: document.getElementById("admin-actions"),
    userActions: document.getElementById("user-actions"),
    movieGrid: document.querySelector(".movie-grid"),
    noMoviesMessage: document.getElementById("no-movies-message"),
    addMovieButton: document.getElementById("add-movie"),
    addMovieUserButton: document.getElementById("add-movie-user"),
    movieForm: document.getElementById("movie-form"),
    genreSelect: document.getElementById("genre"),
    directorSelect: document.getElementById("director"),
    sortSelect: document.getElementById("sort"),
    applyFiltersButton: document.getElementById("applyFilters"),
    saveButton: document.getElementById("save-button"),
    updateButton: document.getElementById("update-button"),
    deleteButton: document.getElementById("delete-button"),
    movieTableBody: document.getElementById("movie-table-body"),
    uploadPosterButton: document.getElementById("upload-poster-btn"),
    posterInput: document.getElementById("poster"),
    posterStatus: document.getElementById("poster-status"),
    directorModal: document.getElementById("director-modal"),
    actorsModal: document.getElementById("actors-modal"),
    selectedDirectors: document.getElementById("selected-directors"),
    selectedActors: document.getElementById("selected-actors"),
    genreModal: document.getElementById("genre-modal"),
    selectedGenres: document.getElementById("selected-genres"),
    statisticFilms: document.getElementById("statistic_films") // Переименовал StaticFilms на statisticFilms для соответствия HTML
};

let selectedGenresList = [];
let isLoginMode = true;
let allMovies = [];
let allGenres = [];
let allDirectors = [];
let allActors = [];
let selectedMovieId = null;
let uploadedPoster = null;
let selectedDirectorsList = [];
let selectedActorsList = [];
elements.statisticFilms.onclick = ShowStaticsFilms;
function ShowStaticsFilms(){
    window.location = "statistics.html";
}
async function addGenre() {
    const genreName = prompt("Введите название жанра:");
    if (!genreName) return;

    try {
        const response = await fetch(`${BASE_URL}/api/genres`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ name: genreName })
        });

        const result = await response.json();
        if (response.ok && result.success) {
            alert(result.message);
            await loadMovies();
            populateSelect(elements.genreModal, allGenres, true);
            selectedGenresList.push(genreName);
            updateSelectedItems(elements.selectedGenres, selectedGenresList, "genres");
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error("Ошибка при добавлении жанра:", error);
        alert(`Ошибка запроса: ${error.message}`);
    }
}

async function addDirector() {
    const firstName = prompt("Введите имя режиссёра:");
    const lastName = prompt("Введите фамилию режиссёра:");
    if (!firstName || !lastName) return;

    try {
        const response = await fetch(`${BASE_URL}/api/directors`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ firstName, lastName })
        });

        const result = await response.json();
        if (response.ok && result.success) {
            alert(result.message);
            await loadMovies();
            populateSelect(elements.directorModal, allDirectors, true);
            const fullName = `${firstName} ${lastName}`.trim();
            selectedDirectorsList.push(fullName);
            updateSelectedItems(elements.selectedDirectors, selectedDirectorsList, "directors");
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error("Ошибка при добавлении режиссёра:", error);
        alert(`Ошибка запроса: ${error.message}`);
    }
}

async function addActor() {
    const firstName = prompt("Введите имя актёра:");
    const lastName = prompt("Введите фамилию актёра:");
    if (!firstName || !lastName) return;

    try {
        const response = await fetch(`${BASE_URL}/api/actors`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ firstName, lastName })
        });

        const result = await response.json();
        if (response.ok && result.success) {
            alert(result.message);
            await loadMovies();
            populateSelect(elements.actorsModal, allActors, true);
            const fullName = `${firstName} ${lastName}`.trim();
            selectedActorsList.push(fullName);
            updateSelectedItems(elements.selectedActors, selectedActorsList, "actors");
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error("Ошибка при добавлении актёра:", error);
        alert(`Ошибка запроса: ${error.message}`);
    }
}

function checkAuth() {
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const login = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload["name"];
            const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
            const userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
            
            if (!login) throw new Error("Логин не найден в токене");

            console.log("Роль пользователя:", role); // Добавляем вывод роли в консоль

            elements.userLoginSpan.textContent = login;
            elements.userInfo.style.display = "inline";
            elements.authButton.textContent = "Выйти";
            elements.authButton.onclick = logout;

            if (role === "admin") {
                elements.adminActions.style.display = "block";
                elements.userActions.style.display = "none";
            } else if (role === "user") {
                elements.adminActions.style.display = "none";
                elements.userActions.style.display = "block";
            }

            localStorage.setItem("userId", userId);
        } catch (e) {
            console.error("Ошибка декодирования токена:", e);
            logout();
        }
    } else {
        elements.userInfo.style.display = "none";
        elements.authButton.textContent = "Войти";
        elements.authButton.onclick = showAuthModal;
        elements.adminActions.style.display = "none";
        elements.userActions.style.display = "none";
        console.log("Роль пользователя: не авторизован"); // Выводим сообщение, если токена нет
    }
}

function showAuthModal() {
    elements.authModal.style.display = "block";
    elements.modalTitle.textContent = "Авторизация";
    elements.submitButton.textContent = "Войти";
    isLoginMode = true;
    elements.authError.textContent = "";
}

Array.from(elements.closeModalButtons).forEach(button => {
    button.onclick = function() {
        const modal = this.closest(".modal");
        if (modal) modal.style.display = "none";
    };
});

elements.switchAuth.onclick = function(e) {
    e.preventDefault();
    isLoginMode = !isLoginMode;
    elements.modalTitle.textContent = isLoginMode ? "Авторизация" : "Регистрация";
    elements.submitButton.textContent = isLoginMode ? "Войти" : "Зарегистрироваться";
    elements.switchAuth.textContent = isLoginMode ? "Перейти к регистрации" : "Перейти к авторизации";
    elements.authError.textContent = "";
};

function logout() {
    localStorage.removeItem("jwtToken");
    localStorage.removeItem("userId");
    token = null;
    checkAuth();
    loadMovies();
}

elements.authForm.onsubmit = async function(e) {
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
            elements.authModal.style.display = "none";
            checkAuth();
            loadMovies();
        } else {
            elements.authError.textContent = text;
        }
    } catch (error) {
        elements.authError.textContent = `Ошибка запроса: ${error.message}`;
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

        populateSelect(elements.genreSelect, allGenres);
        populateSelect(elements.directorSelect, allDirectors);
        populateSelect(elements.directorModal, allDirectors, true);
        populateSelect(elements.actorsModal, allActors, true);
        populateSelect(elements.genreModal, allGenres, true);
        
        applyFilters();
        populateMovieTable();
    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
        elements.movieGrid.innerHTML = "";
        elements.noMoviesMessage.style.display = "block";
        elements.noMoviesMessage.textContent = "Ошибка загрузки данных";
    }
}

function populateSelect(selectElement, items, addNewOption = false) {
    if (!selectElement) {
        console.error("Элемент select не найден");
        return;
    }

    // Проверяем, является ли текущий элемент genreSelect
    const isGenreSelect = selectElement === elements.genreSelect;

    // Если это genreSelect и нет данных, оставляем только "Выберите" без "Нету данных"
    if (isGenreSelect) {
        selectElement.innerHTML = '<option value="">Выберите</option>' + 
            items.map(item => `<option value="${item}">${item}</option>`).join('');
    } else {
        // Для остальных select сохраняем текущую логику
        selectElement.innerHTML = items.length === 0
            ? '<option value="">Нету данных</option>'
            : '<option value="">Выберите</option>' + items.map(item => `<option value="${item}">${item}</option>`).join('');
    }

    if (addNewOption) {
        selectElement.innerHTML += '<option value="add-new">Добавить новый...</option>';
    }
}

elements.uploadPosterButton.onclick = function() {
    elements.posterInput.click();
};

elements.posterInput.addEventListener("change", function(e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function(event) {
            uploadedPoster = event.target.result;
            elements.posterStatus.textContent = "Постер успешно загружен!";
            elements.posterStatus.style.color = "green";
        };
        reader.onerror = function() {
            elements.posterStatus.textContent = "Ошибка при загрузке постера";
            elements.posterStatus.style.color = "red";
        };
        reader.readAsDataURL(file);
    }
});

function applyFilters() {
    const genreFilter = elements.genreSelect.value;
    const directorFilter = elements.directorSelect.value;
    const sortOrder = elements.sortSelect.value;

    let filteredMovies = [...allMovies];

    if (selectedGenresList.length > 0) {
        filteredMovies = filteredMovies.filter(movie => 
            movie.genres && selectedGenresList.some(genre => 
                movie.genres.split(", ").includes(genre))
        );
    } else if (genreFilter && genreFilter !== "Нету данных") {
        filteredMovies = filteredMovies.filter(movie => 
            movie.genres && movie.genres.split(", ").includes(genreFilter));
    }

    if (directorFilter && directorFilter !== "Нету данных") {
        filteredMovies = filteredMovies.filter(movie => 
            movie.directors && movie.directors.includes(directorFilter));
    }

    if (sortOrder === "asc") {
        filteredMovies.sort((a, b) => parseInt(a.year) - parseInt(b.year));
    } else if (sortOrder === "desc") {
        filteredMovies.sort((a, b) => parseInt(b.year) - parseInt(b.year));
    }

    elements.movieGrid.innerHTML = "";
    if (filteredMovies.length === 0) {
        elements.noMoviesMessage.style.display = "block";
        elements.noMoviesMessage.textContent = "Фильмы не найдены.";
        return;
    }

    elements.noMoviesMessage.style.display = "none";
    filteredMovies.forEach(movie => {
        const movieCard = document.createElement("div");
        movieCard.classList.add("movie-card");
        movieCard.style.cursor = "pointer";
        movieCard.innerHTML = `
            <h3>${movie.title}</h3>
            <p>Год: ${movie.year}</p>
            <p>Жанр: ${movie.genres || "Нет жанров"}</p>
            <p>Режиссеры: ${movie.directors || "Нет режиссёров"}</p>
            <p>Актеры: ${movie.actors || "Нет актёров"}</p>
            ${movie.poster ? `<img src="${movie.poster}" alt="${movie.title}" style="max-width: 100%; height: auto;">` : ""}
        `;
        movieCard.addEventListener("click", () => {
            // Переход на страницу подробностей с передачей ID фильма
            window.location.href = `more.html?id=${movie.id}`;
        });
        elements.movieGrid.appendChild(movieCard);
    });
}

function showMovieModal() {
    elements.movieModal.style.display = "block";
    resetForm();
    populateMovieTable();
}

function resetForm() {
    selectedMovieId = null;
    uploadedPoster = null;
    selectedDirectorsList = [];
    selectedActorsList = [];
    selectedGenresList = [];

    document.getElementById("movie-id").value = "";
    document.getElementById("title").value = "";
    document.getElementById("year").value = "";
    document.getElementById("description").value = "";
    elements.posterInput.value = "";
    elements.posterStatus.textContent = "";

    elements.saveButton.style.display = "inline";
    elements.updateButton.style.display = "none";
    elements.deleteButton.style.display = "none";

    populateSelect(elements.genreModal, allGenres, true);
    updateSelectedItems(elements.selectedGenres, selectedGenresList, "genres");
    updateSelectedItems(elements.selectedDirectors, selectedDirectorsList, "directors");
    updateSelectedItems(elements.selectedActors, selectedActorsList, "actors");
}

function updateSelectedItems(container, items, listType) {
    container.innerHTML = "";
    if (!items || items.length === 0) {
        container.innerHTML = "<span>Не выбрано</span>";
    } else {
        items.forEach(item => {
            const div = document.createElement("div");
            div.className = "selected-item";
            div.innerHTML = `<span>${item}</span><span class="remove-item">×</span>`;
            div.querySelector(".remove-item").onclick = () => {
                if (listType === "genres") {
                    selectedGenresList = selectedGenresList.filter(i => i !== item);
                    updateSelectedItems(container, selectedGenresList, "genres");
                } else if (listType === "directors") {
                    selectedDirectorsList = selectedDirectorsList.filter(i => i !== item);
                    updateSelectedItems(container, selectedDirectorsList, "directors");
                } else if (listType === "actors") {
                    selectedActorsList = selectedActorsList.filter(i => i !== item);
                    updateSelectedItems(container, selectedActorsList, "actors");
                }
                applyFilters();
            };
            container.appendChild(div);
        });
    }
}

if (elements.genreModal) {
    elements.genreModal.onchange = function() {
        const value = this.value;
        if (value === "add-new") {
            addGenre();
            this.value = "";
        } else if (value && value !== "" && !selectedGenresList.includes(value)) {
            selectedGenresList.push(value);
            updateSelectedItems(elements.selectedGenres, selectedGenresList, "genres");
            this.value = "";
            applyFilters();
        }
    };
}

if (elements.directorModal) {
    elements.directorModal.onchange = function() {
        const value = this.value;
        if (value === "add-new") {
            addDirector();
            this.value = "";
        } else if (value && !selectedDirectorsList.includes(value)) {
            selectedDirectorsList.push(value);
            updateSelectedItems(elements.selectedDirectors, selectedDirectorsList, "directors");
            this.value = "";
        }
    };
}

if (elements.actorsModal) {
    elements.actorsModal.onchange = function() {
        const value = this.value;
        if (value === "add-new") {
            addActor();
            this.value = "";
        } else if (value && !selectedActorsList.includes(value)) {
            selectedActorsList.push(value);
            updateSelectedItems(elements.selectedActors, selectedActorsList, "actors");
            this.value = "";
        }
    };
}

function populateMovieTable() {
    elements.movieTableBody.innerHTML = "";
    
    if (allMovies.length === 0) {
        elements.movieTableBody.innerHTML = '<tr><td colspan="8">Фильмы не найдены</td></tr>';
        return;
    }

    let role = null;
    let userId = localStorage.getItem("userId");
    let userLogin = null;

    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
            userLogin = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload["name"];
        } catch (e) {
            console.error("Ошибка декодирования токена:", e);
        }
    }

    console.log("Роль:", role, "User ID:", userId, "Логин:", userLogin); // Диагностика

    let moviesToShow = [];
    if (role === "admin") {
        moviesToShow = allMovies; // Админ видит все фильмы
    } else if (role === "user" && userLogin) {
        moviesToShow = allMovies.filter(movie => {
            // Предполагаем, что movie.creator содержит логин создателя
            const isCreatorMatch = movie.creator === userLogin;
            console.log(`Фильм: ${movie.title}, Creator: ${movie.creator}, Совпадение: ${isCreatorMatch}`);
            return isCreatorMatch;
        });
    } else {
        elements.movieTableBody.innerHTML = '<tr><td colspan="8">Авторизуйтесь для просмотра</td></tr>';
        return;
    }

    if (moviesToShow.length === 0) {
        elements.movieTableBody.innerHTML = '<tr><td colspan="8">Фильмы не найдены</td></tr>';
        return;
    }

    moviesToShow.forEach(movie => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${movie.title || "Без названия"}</td>
            <td>${movie.genres || "Нет жанров"}</td>
            <td>${movie.directors || "Нет режиссёров"}</td>
            <td>${movie.year || "Нет года"}</td>
            <td>${movie.actors || "Нет актёров"}</td>
            <td>${movie.poster ? '<img src="' + movie.poster + '" style="max-width: 50px;">' : "Нет"}</td>
            <td>${movie.description || "Нет описания"}</td>
            <td>${movie.creator || "Неизвестно"}</td>
        `;
        row.style.cursor = "pointer";
        row.addEventListener("click", () => {
            editMovie(movie.id);
        });
        elements.movieTableBody.appendChild(row);
    });
}

function editMovie(movieId) {
    const movie = allMovies.find(m => m.id === movieId);
    if (movie) {
        selectedMovieId = movieId;
        uploadedPoster = movie.poster;
        
        selectedGenresList = (movie.genres && movie.genres.trim() !== "") 
            ? movie.genres.split(", ").map(g => g.trim()) 
            : [];
        selectedDirectorsList = (movie.directors && movie.directors.trim() !== "") 
            ? movie.directors.split(", ").map(d => d.trim()) 
            : [];
        selectedActorsList = (movie.actors && movie.actors.trim() !== "") 
            ? movie.actors.split(", ").map(a => a.trim()) 
            : [];
        
        document.getElementById("movie-id").value = movie.id;
        document.getElementById("title").value = movie.title || "";
        document.getElementById("year").value = movie.year || "";
        document.getElementById("description").value = movie.description || "";
        
        elements.saveButton.style.display = "none";
        elements.updateButton.style.display = "inline";
        elements.deleteButton.style.display = "inline";
        
        elements.posterStatus.textContent = movie.poster ? "Текущий постер загружен" : "Постер не загружен";
        elements.posterStatus.style.color = movie.poster ? "green" : "black";
        
        updateSelectedItems(elements.selectedGenres, selectedGenresList, "genres");
        updateSelectedItems(elements.selectedDirectors, selectedDirectorsList, "directors");
        updateSelectedItems(elements.selectedActors, selectedActorsList, "actors");
        applyFilters();
    }
}

elements.addMovieButton.onclick = showMovieModal;
elements.addMovieUserButton.onclick = showMovieModal;
elements.applyFiltersButton.onclick = applyFilters;

elements.movieForm.onsubmit = async function(e) {
    e.preventDefault();

    // Собираем данные из формы и выбранных элементов
    const title = document.getElementById("title").value;
    const year = document.getElementById("year").value;
    const description = document.getElementById("description").value || "";
    const genres = selectedGenresList.length > 0 ? selectedGenresList : [];
    const directors = selectedDirectorsList.length > 0 ? selectedDirectorsList : [];
    const actors = selectedActorsList.length > 0 ? selectedActorsList : [];
    const poster = uploadedPoster || null;

    // Проверяем, что жанры выбраны
    if (genres.length === 0) {
        alert("Пожалуйста, выберите хотя бы один жанр.");
        return;
    }

    const movieData = {
        title,
        genres,
        directors,
        year,
        description,
        actors,
        poster
    };

    console.log("Отправляемые данные:", JSON.stringify(movieData));

    try {
        const response = await fetch(`${BASE_URL}/api/movies`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(movieData)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText);
        }
        
        elements.movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при добавлении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

elements.updateButton.onclick = async function() {
    if (!selectedMovieId) return;

    const title = document.getElementById("title").value;
    const year = document.getElementById("year").value;
    const description = document.getElementById("description").value;
    const genres = selectedGenresList;
    const directors = selectedDirectorsList;
    const actors = selectedActorsList;
    const poster = uploadedPoster;

    // Проверяем, что жанры выбраны
    if (genres.length === 0) {
        alert("Пожалуйста, выберите хотя бы один жанр.");
        return;
    }

    const movieData = {
        title,
        genres,
        directors,
        year,
        description,
        actors,
        poster
    };

    try {
        const response = await fetch(`${BASE_URL}/api/movies/${selectedMovieId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(movieData)
        });

        if (!response.ok) throw new Error(await response.text());
        
        elements.movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при обновлении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

elements.deleteButton.onclick = async function() {
    if (!selectedMovieId || !confirm("Вы уверены, что хотите удалить этот фильм?")) return;

    try {
        const response = await fetch(`${BASE_URL}/api/movies/${selectedMovieId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error(await response.text());
        
        elements.movieModal.style.display = "none";
        loadMovies();
    } catch (error) {
        console.error("Ошибка при удалении фильма:", error);
        alert(`Ошибка: ${error.message}`);
    }
};

window.onload = function() {
    checkAuth();
    loadMovies();
};