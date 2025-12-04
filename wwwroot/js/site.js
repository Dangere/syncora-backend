// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


let hidePassword = true;
function TogglePassword() {
    hidePassword = !hidePassword;


    /** @type {HTMLInputElement} */
    const passwordFieldInput = document.getElementById("passwordField");
    /** @type {HTMLInputElement} */
    const confirmPasswordFieldInput = document.getElementById("confirmPasswordField");

    const passwordFieldType = hidePassword ? "password" : "text";

    passwordFieldInput.type = passwordFieldType;
    confirmPasswordFieldInput.type = passwordFieldType;


    const iconVisibilityClass = !hidePassword ? "bi-eye-slash" : "bi-eye";


    /** @type {HTMLInputElement} */
    const passwordFieldIcon = document.getElementById("togglePasswordIcon");
    /** @type {HTMLInputElement} */
    const confirmPasswordFieldIcon = document.getElementById("toggleConfirmPasswordIcon");


    passwordFieldIcon.className = "bi " + iconVisibilityClass;
    confirmPasswordFieldIcon.className = "bi " + iconVisibilityClass;



}