document.getElementById('telegramForm').onsubmit = async function (event) {
    event.preventDefault();

    const formData = new FormData(this);
    const response = await fetch('/api/telegram/send', {
        method: 'POST',
        body: formData
    });

    const result = await response.json();
    const messageDiv = document.getElementById('resultMessage');

    if (response.ok) {
        messageDiv.innerHTML = `<div class="alert alert-success">${result.Message}</div>`;
    } else {
        messageDiv.innerHTML = `<div class="alert alert-danger">${result.Message} - ${result.Details}</div>`;
    }
};