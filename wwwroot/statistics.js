// statistics.js
document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = '/api'; // Замените на URL вашего API
    const genreList = document.getElementById('genre-list');
    const directorList = document.getElementById('director-list');
    const sortYearSelect = document.getElementById('sort-year');
    const yearList = document.getElementById('year-list');

    // Функция для получения токена из localStorage
    function getAuthToken() {
        return localStorage.getItem('authToken');
    }
    function NavigateHomepage() {
        window.location = "homepage.html";
    }
    // Функция для получения статистики по жанрам
    async function fetchGenreStats() {
        const token = getAuthToken();
        try {
            const response = await fetch(`${API_BASE_URL}/statistics/genres`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                console.error('Ошибка при получении статистики по жанрам:', response.status);
                return;
            }

            const genres = await response.json();

            // Отображение статистики в виде списка
            genres.forEach(genre => {
                const li = document.createElement('li');
                li.textContent = `${genre.name}: ${genre.count} фильмов`; // Предполагается, что API возвращает объекты с полями name и count
                genreList.appendChild(li);
            });

        } catch (error) {
            console.error('Ошибка при получении статистики по жанрам:', error);
        }
    }

    // Функция для получения статистики по режиссерам
    async function fetchDirectorStats() {
        const token = getAuthToken();
        try {
            const response = await fetch(`${API_BASE_URL}/statistics/directors`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                console.error('Ошибка при получении статистики по режиссерам:', response.status);
                return;
            }

            const directors = await response.json();

            // Отображение статистики в виде списка
            directors.forEach(director => {
                const li = document.createElement('li');
                li.textContent = `${director.name}: ${director.count} фильмов`; // Предполагается, что API возвращает объекты с полями name и count
                directorList.appendChild(li);
            });

        } catch (error) {
            console.error('Ошибка при получении статистики по режиссерам:', error);
        }
    }

    // Функция для сортировки фильмов по году
    async function sortMoviesByYear(order = 'asc') {
        const token = getAuthToken();
        try {
            const response = await fetch(`${API_BASE_URL}/movies?sortByYear=${order}`, { // Используйте API для получения отсортированного списка
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                console.error('Ошибка при получении отсортированного списка фильмов:', response.status);
                return;
            }

            const movies = await response.json();

            // Очистка списка
            yearList.innerHTML = '';

            // Отображение отсортированного списка фильмов
            movies.forEach(movie => {
                const li = document.createElement('li');
                li.textContent = `${movie.title} (${movie.year})`;
                yearList.appendChild(li);
            });

        } catch (error) {
            console.error('Ошибка при получении отсортированного списка фильмов:', error);
        }
    }

    // Обработчик изменения выбора сортировки
    sortYearSelect.addEventListener('change', () => {
        const sortOrder = sortYearSelect.value;
        sortMoviesByYear(sortOrder);
    });

    // Инициализация: получение статистики при загрузке страницы
    fetchGenreStats();
    fetchDirectorStats();
    sortMoviesByYear(); // По умолчанию сортировка по возрастанию
});