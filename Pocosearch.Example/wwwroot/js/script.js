function getArticleTemplate() {
    const originalArticle = document.querySelector('.article');
    const articleTemplate = originalArticle.cloneNode(true);
    originalArticle.remove();

    return articleTemplate;
}

function renderArticles(articles, articleTemplate) {
    const searchResultContainer = document.getElementById('searchResults');
    searchResultContainer.innerHTML = '';

    for (const article of articles) {
        const articleContainer = articleTemplate.cloneNode(true);
        articleContainer.querySelector('b').innerText = article.document.title;
        articleContainer.querySelector('p').innerText = article.document.body;
        articleContainer.querySelector('i').innerText = article.document.publishedOn;
        searchResultContainer.appendChild(articleContainer);
    }
}

async function search(query, articleTemplate, excludeBody, boostTitle, searchAsYouType) {
    const response = await fetch('/articles?' + new URLSearchParams({
        search: query,
        excludeBody,
        boostTitle,
        searchAsYouType
    }));

    const articles = await response.json();
    renderArticles(articles, articleTemplate);
}

function initialize() {
    const searchInput = document.getElementById('searchInput');
    const seedButton = document.getElementById('seedButton');
    const searchButton = document.getElementById('searchButton');
    const excludeBodyCheckbox = document.getElementById('excludeBody');
    const boostTitleCheckbox = document.getElementById('boostTitle');
    const searchAsYouType = document.getElementById('searchAsYouType');

    const articleTemplate = getArticleTemplate();

    const searchListener = (e) => {
        search(
            searchInput.value, 
            articleTemplate, 
            excludeBodyCheckbox.checked, 
            boostTitleCheckbox.checked,
            searchAsYouType.checked
        );
    };

    searchButton.addEventListener('click', searchListener);

    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !searchAsYouType.checked) {
            searchListener(e);
        }
    });

    searchAsYouType.addEventListener('change', (e) => {
        if (searchAsYouType.checked) {
            searchInput.addEventListener('input', searchListener);
            searchButton.disabled = true;
        } else {
            searchInput.removeEventListener('input', searchListener);
            searchButton.disabled = false;
        }
    });

    seedButton.addEventListener('click', (e) => {
        fetch('/articles/seed', { method: 'post' });
    });
}

initialize();
