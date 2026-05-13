document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const searchDropdown = document.getElementById('searchDropdown');
    const searchResults = document.getElementById('searchResults');

    if (!searchInput) return;

    searchInput.addEventListener('input', async function() {
        const query = this.value.trim();

        if (query.length < 2) {
            searchDropdown.style.display = 'none';
            return;
        }

        try {
            const response = await fetch(`/api/search?q=${encodeURIComponent(query)}`);
            const data = await response.json();

            if (data.length === 0) {
                searchResults.innerHTML = '<div class="search-no-results">No vets or shelters found</div>';
            } else {
                searchResults.innerHTML = data.map(item => `
                    <a href="${item.url}" class="search-result-item">
                        <div class="search-result-icon">
                            ${item.type === 'Vet' ? '<i class="bi bi-hospital"></i>' : '<i class="bi bi-heart"></i>'}
                        </div>
                        <div class="search-result-info">
                            <div class="search-result-name">${item.name}</div>
                            <div class="search-result-type">${item.type}</div>
                        </div>
                    </a>
                `).join('');
            }

            searchDropdown.style.display = 'block';
        } catch (error) {
            console.error('Search error:', error);
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function(event) {
        if (!event.target.closest('.topbar-search')) {
            searchDropdown.style.display = 'none';
        }
    });
});

@section Scripts {
    <script src="~/js/search.js"></script>
}document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const searchDropdown = document.getElementById('searchDropdown');
    const searchResults = document.getElementById('searchResults');

    if (!searchInput) return;

    searchInput.addEventListener('input', async function() {
        const query = this.value.trim();

        if (query.length < 2) {
            searchDropdown.style.display = 'none';
            return;
        }

        try {
            const response = await fetch(`/api/search?q=${encodeURIComponent(query)}`);
            const data = await response.json();

            if (data.length === 0) {
                searchResults.innerHTML = '<div class="search-no-results">No vets or shelters found</div>';
            } else {
                searchResults.innerHTML = data.map(item => `
                    <a href="${item.url}" class="search-result-item">
                        <div class="search-result-icon">
                            ${item.type === 'Vet' ? '<i class="bi bi-hospital"></i>' : '<i class="bi bi-heart"></i>'}
                        </div>
                        <div class="search-result-info">
                            <div class="search-result-name">${item.name}</div>
                            <div class="search-result-type">${item.type}</div>
                        </div>
                    </a>
                `).join('');
            }

            searchDropdown.style.display = 'block';
        } catch (error) {
            console.error('Search error:', error);
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function(event) {
        if (!event.target.closest('.topbar-search')) {
            searchDropdown.style.display = 'none';
        }
    });
});