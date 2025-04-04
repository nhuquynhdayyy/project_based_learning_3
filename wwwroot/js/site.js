// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleLike(icon) {
    const likeCount = icon.parentElement.nextElementSibling.querySelector('.like-count');
    let count = parseInt(likeCount.innerText);

    if (icon.classList.contains("liked")) {
        icon.classList.remove("liked");
        icon.innerHTML = "🤍"; // Trái tim trắng
        likeCount.innerText = count - 1;
    } else {
        icon.classList.add("liked");
        icon.innerHTML = "❤️"; // Trái tim đỏ
        likeCount.innerText = count + 1;
    }
}
