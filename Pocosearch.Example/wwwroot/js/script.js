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

async function search(articleTemplate) {
    const response = await fetch('/articles?' + new URLSearchParams({
        search: searchInput.value
    }));

    const articles = await response.json();
    renderArticles(articles, articleTemplate);
}

function initialize() {
    const searchInput = document.getElementById('searchInput');
    //const seedButton = document.getElementById('seed');
    const searchButton = document.getElementById('searchButton');

    const articleTemplate = getArticleTemplate();

    searchButton.addEventListener('click', async (e) => {
        search(articleTemplate);
    });

    searchInput.addEventListener('keypress', async (e) => {
        if (e.key === 'Enter') {
            search(articleTemplate);
        }
    });

    // @TODO: add back when search as you type checkbox is enabled
    /*
    searchInput.addEventListener('input', (e) => {
        console.log(e.target.value);
    });
    */

    /*
    seedButton.addEventListener('click', (e) => {
        fetch('/articles/seed', {
            method: 'post'
        });
    });
    */
}

initialize();
