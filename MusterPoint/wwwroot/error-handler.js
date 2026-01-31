// Error UI Handler
document.addEventListener('DOMContentLoaded', function () {
    const errorUi = document.getElementById('blazor-error-ui');
    const dismissBtn = errorUi?.querySelector('.dismiss');
    const reloadBtn = errorUi?.querySelector('.reload');

    // Handle dismiss button
    if (dismissBtn) {
        dismissBtn.addEventListener('click', function () {
       if (errorUi) {
      errorUi.style.display = 'none';
      }
     });
    }

    // Handle reload button
    if (reloadBtn) {
        reloadBtn.addEventListener('click', function () {
    location.reload();
        });
    }

    // Listen for Blazor errors
    window.addEventListener('error', function (event) {
        if (errorUi) {
            errorUi.classList.add('show');
        }
    });
});
