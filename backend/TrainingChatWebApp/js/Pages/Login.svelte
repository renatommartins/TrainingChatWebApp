<script>
    import Materialize from "materialize-css";
    
    import RestApi from "Modules/RestApi.svelte";
    
    import TextInput from "Modules/TextInput.svelte";
    import PasswordInput from "Modules/PasswordInput.svelte";
    import Button from "Modules/Button.svelte";
    import Alert from "Modules/Alert.svelte";
    
    let Login;

    let username = "";
    let password = "";
    let alert_text = "";
    let alert_is_visible = false;
    
    let login_button_handler = function() {
        alert_is_visible = false;

        Login(
            username,
            password,
            function (response) {
                localStorage.setItem("sessionToken", response.session);
                window.location.href = "/chat.html"
            },
            function (response) {
                switch (response.statusCode) {
                    case 401:
                        password = '';
                        alert_text = "Incorrect user or password";
                        alert_is_visible = true;
                        break;
                    case 500:
                        alert_text = "Our server is out";
                        alert_is_visible = true;
                }
            }
        );
    };
    
    let signup_button_handler = function() {
        window.location.href = '/signup.html';
    };
</script>
<RestApi bind:Login></RestApi>

<div class="container app-body">
    <h1 class="row">Aravipac Chat Corp</h1>
    <h2 class="row">Login</h2>
    <div class="row">
        <TextInput
                bind:input_text={username}
                id="inUser"
                label_text="User">
        </TextInput>
        <PasswordInput
                bind:input_text={password}
                id="inPassword"
                label_text="Password">
        </PasswordInput>
        <div class="row">
            <div class="col s10 offset-s1">
                <Alert bind:alert_text={alert_text} is_visible={alert_is_visible}></Alert>
            </div>
        </div>
        <div class="row">
            <div class="col s2">
                <Button button_label="Login" on:click={login_button_handler}></Button>
            </div>
            <div class="col s2 offset-s8">
                <Button button_label="Sign Up" on:click={signup_button_handler}></Button>
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
