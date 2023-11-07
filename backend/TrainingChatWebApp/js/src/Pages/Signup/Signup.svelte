<script>
import Materialize from "materialize-css"

import TextInput from "../../Modules/TextInput.svelte";
import PasswordInput from "../../Modules/PasswordInput.svelte";
import Alert from "../../Modules/Alert.svelte";
import Button from "../../Modules/Button.svelte";

let name = "";
let name_enabled = true;

let username = "";
let username_enabled = true;

let password = "";
let password_enabled = true;

let confirm_password = "";
let confirm_password_enabled = true;

let alert_text = "";
let alert_is_visible = false;
let alert_success = "#A5D6A7";
let alert_failure = "#EF9A9A";
let alert_current_color = alert_success;

let signup_button_enabled = true;
let signup_button_handler = function (){
    if(password !== confirm_password)
    {
        password = '';
        confirm_password = '';
        alert_text = "Different password";
        alert_current_color = alert_failure;
        alert_is_visible = true;
        return;
    }

    let httpRequest = new XMLHttpRequest();
    let url = '/signup';
    let body = JSON.stringify({Name: name, Username: username, Password: password});
    console.log(body);
    httpRequest.open("POST", url);
    httpRequest.setRequestHeader("Content-type","application/json");
    httpRequest.onloadend = function (e) {
        switch (httpRequest.status) {
            case 200:
                console.log("Sucesso");
                alert_text = "User created successfully\nRedirecting to Login Page";
                alert_is_visible = true;
                signup_button_enabled = false;
                name_enabled = false;
                username_enabled = false;
                password_enabled = false;
                confirm_password_enabled = false;

                alert_current_color = alert_success;
                alert_is_visible = true;

                setTimeout(function (){
                    window.location.href = "/login.html"
                }, 5000)
                break;
            case 409:
                password = '';
                alert_text = "User already exists";
                alert_current_color = alert_failure;
                alert_is_visible = true;
                console.log("Usuário existente");
                break;
            case 500:
                console.log(".");
        }
    };
    httpRequest.send(body);
};

let login_button_enabled = true;
let login_button_handler = function (){
    window.location.href = "/login.html";
};

</script>

<div class="container app-body">
    <h1 class="row">Aravipac Chat Corp</h1>
    <h2 class="row">Sign Up</h2>
    <div class="row">
        <TextInput
            bind:input_text={name}
            id="inName"
            label_text="Name"
            placeholder="Name"
            bind:enabled={name_enabled}>>
        </TextInput>
        <TextInput
            bind:input_text={username}
            id="inUser"
            label_text="User"
            placeholder="User"
            bind:enabled={username_enabled}>
        </TextInput>
        <PasswordInput
            bind:input_text={password}
            id="inPassword"
            label_text="Password"
            placeholder="Password"
            bind:enabled={password_enabled}>>
        </PasswordInput>
        <PasswordInput
            bind:input_text={confirm_password}
            id="inConfirmPassword"
            label_text="Confirm Password"
            placeholder="Confirm Password"
            bind:enabled={confirm_password_enabled}>>
        </PasswordInput>
        <div class="row">
            <div class="col s10 offset-s1">
                <Alert
                    bind:alert_text={alert_text}
                    is_visible={alert_is_visible}
                    --color={alert_current_color}>
                </Alert>
            </div>
        </div>
        <div class="row">
            <div class="col s2">
                <Button
                    button_label="Sign Up"
                    bind:enabled={signup_button_enabled}
                    on:click={signup_button_handler}>
                </Button>
            </div>
            <div class="col s2 offset-s8">
                <Button
                    button_label="Login"
                    bind:enabled={login_button_enabled}
                    on:click={login_button_handler}>
                </Button>
            </div>
        </div>
    </div>
</div>

<style>
    .app-body {
        overflow-y: hidden;
    }
    h1 {
        font-size: 30px;
        text-align: center;
        margin-bottom: 10%;
    }
    h2 {
        font-size: 24px;
        text-align: left;
    }
</style>
