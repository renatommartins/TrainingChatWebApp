<script>
    import Materialize from "materialize-css"
    import { onMount } from "svelte";
    
    import RestApi from "Modules/RestApi.svelte";
    import WebsocketApi from "Modules/WebsocketApi.svelte";
    import Button from "Modules/Button.svelte";
    import TextInput from "Modules/TextInput.svelte";
    
    let Logout;

    let Connect;
    let CreateChatRoom;
    let ListChatRoom;
    let JoinChatRoom;
    let LeaveChatRoom;
    let SendChatRoomMessage;
    
    let createModal = undefined;
    let disconnectedModal = undefined;

    let user_id = 0;
    let name = '';
    let username = '';

    let create_room_name = '';
    let is_joined = false;

    let list_response_handler = function (list_response) { rooms = list_response; };

    onMount(() => {
        let elems = document.querySelectorAll('.modal');
        let elemsSideNav = document.querySelectorAll('.sidenav');
        let instances = M.Modal.init(elems);
        let instancesSideNav = M.Sidenav.init(elemsSideNav);
        window.modalInstances = instances;
        window.sideNavInstances = instancesSideNav;

        window.modalInstances.forEach(element => {
            switch (element.id) {
                case "create-room-modal": {
                    createModal = element;
                }
                break;
                case "disconnectedModal": {
                    disconnectedModal = element;
                    element.options.dismissible = false;
                }
                break;
                default: break;
            }
        });
        
        Connect(
            localStorage.getItem("sessionToken"),
            () => { ListChatRoom(list_response_handler) },
            () => { disconnectedModal.open(); },
            () => { disconnectedModal.open(); });
    });
    
    /*let rooms = [
        { Id: 1, Name: "123123" },
        { Id: 2, Name: "os cara tão vendo sim" },
        { Id: 3, Name: "mal pelo vacilo" },
    ];
    let current_room = {
        name: "Test Name for Room",
        users: [ "Capeta Liso", "bum de fora" ],
        messages: [ "aeHOOOOO", "aravipac", "os cara tá nem vendo" ],
    };*/

    let rooms = [];
    let current_room = {
        name: "",
        users: [],
        messages: [],
    };
    
    let message_to_send;
    
    const logout_callback = function () {
        localStorage.clear();
        window.location.href = 'login.html'
    }
    
    let logout_handler = function () {
        Logout( localStorage.getItem("sessionToken"), logout_callback, logout_callback);
    };

    let reconnect_handler = function () {
        window.location.reload();
    };
    
    let receive_message_handler = function (message_notification) {
        let new_message = {
            user_name: message_notification.UserName,
            text: message_notification.Message,
        };
        current_room.messages = [...current_room.messages, new_message];
    };
    
    let user_join_handler = function (joined_notification) {
        let joined_user = {
            Id: joined_notification.Id,
            UserName: joined_notification.UserName,
        };
        current_room.users = [...current_room.users, joined_user];
    };
    
    let user_left_handler = function (leave_notification) {
        current_room.users = current_room.users.filter((user) => {
            return user.Id !== leave_notification.Id;
        });
    };
    
    let create_button_handler = function () {
        CreateChatRoom(
            create_room_name,
            (create_response) => {
                is_joined = true;
                message_to_send = '';
                createModal.close();
                current_room.name = create_response.Name;
                current_room.messages = [];
                current_room.users = [{
                    Id: user_id,
                    UserName: name,
                }];
            },
            receive_message_handler,
            user_join_handler,
            user_left_handler);
    };

    let open_create_modal = function () {
        createModal.open();
    }
    
    let close_create_modal = function () {
        createModal.close();
    };
    
    let refresh_button_handler = function () {
        console.log("Refresh chat room list");
        ListChatRoom(list_response_handler);
    };
    
    let room_join_handler = function (id) {
        console.log("Clicked to join room id " + id);
        JoinChatRoom(
            id,
            (join_response) =>{
                is_joined = true;
                message_to_send = '';
                current_room.name = join_response.Name;
                current_room.messages = [];
                join_response.Users.forEach((user) => {
                    current_room.users.push({
                        Id: user.Id,
                        UserName: user.Name,
                    });
                });
            },
            receive_message_handler,
            user_join_handler,
            user_left_handler
        );
    };

    let leave_button_handler = function () {
        LeaveChatRoom((leave_response) => {
            is_joined = false;
            current_room.name = '';
            current_room.messages = [];
            current_room.users = [];
            message_to_send = '';
        });
    };

    let clear_button_handler = function () {
        message_to_send = '';
    };

    let send_button_handler = function () {
        SendChatRoomMessage(message_to_send);
        message_to_send = '';
    };
    
    let keydown_handler = function (event) {
        if (!event.isComposing && event.key === "Enter") {
            if(is_joined) {
                send_button_handler();
            }
        } else if (event.key === "Escape") {
            message_to_send = '';
        }
    };
</script>
<RestApi bind:Logout></RestApi>
<WebsocketApi
    bind:user_id
    bind:name
    bind:username
    bind:Connect
    bind:CreateChatRoom
    bind:ListChatRoom
    bind:JoinChatRoom
    bind:LeaveChatRoom
    bind:SendChatRoomMessage>
</WebsocketApi>
<link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet">

<div id="create-room-modal" class="modal">
    <div class="modal-content">
        <h5>Create Chat Room</h5>
        <TextInput
                bind:input_text={create_room_name}
                id="inRoomName"
                label_text="Chat Room Name">
        </TextInput>
        <pre id="outAlertRoom"></pre>
        <div class="row aravipac-modalchat">
            <div class="col s2 offset-s8">
                <Button button_label="Create" on:click={create_button_handler}></Button>
            </div>
            <div class="col s2">
                <Button button_label="Close" on:click={close_create_modal}></Button>
            </div>
        </div>
    </div>
</div>
<div id="disconnectedModal" class="modal">
    <div class="modal-content">
        <p style="text-align: center;">Server has disconnected</p>
        <div class="row" style="margin-bottom: 0;">
            <div class="col s3">
                <Button button_label="Logout" on:click={logout_handler}></Button>
            </div>
            <div class="col s3 offset-s6">
                <Button button_label="Reconnect" on:click={reconnect_handler}></Button>
            </div>
        </div>
    </div>
</div>
<div class="sidenav grey" id="slide-out">
    <div class="sidenav-content">
        <img src="aravipacPipoqueiraLogo.png" class="sidenav-logo" alt="Aqui tem uma aravipac"/>
        <p>Welcome, {name}</p>
        <div class="sidenav-filler"></div>
        <div class="sidenav-btn">
            <Button button_label="Logout" on:click={logout_handler}></Button>
        </div>
        <p>Aravipac Production &copy;</p>
    </div>
</div>
<div class="chat-container">
    <div class="grey minimized-sidenav">
        <a
            data-target="slide-out"
            class="waves-effect waves-light sidenav-trigger aravipac-hamburguer">
            <i style="color:black; margin:5px;" class="material-icons">dehaze</i>
        </a>
    </div>
    <div class="spa-container blue-grey lighten-1">
        <div class="chat-room-list" class:hide={is_joined}>
            <div class="chat-list-header">
                <h2 class="chat-list-title">Chat Room List</h2>
                <div class="chat-list-button">
                    <Button button_label="Create Room" on:click={open_create_modal}></Button>
                </div>
                <div class="chat-list-button">
                    <Button button_label="Refresh" on:click={refresh_button_handler}></Button>
                </div>
            </div>
            <div class="chat-room-list-table">
                <table class="highlight">
                    <tbody>
                        {#each rooms as room (room.Id)}
                        <tr>
                            <td on:click={()=>{ room_join_handler(room.Id); }}>{room.Name}</td>
                        </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>
        <div class="joined-chat-room" class:hide={!is_joined}>
            <div class="chat-room-header">
                <h2>{current_room.name}</h2>
                <div class="button">
                    <Button button_label="Leave Room" on:click={leave_button_handler}></Button>
                </div>
            </div>
            <div class="chat-room-messages-container">
                <div class="chat-room-messages">
                    <table class="strip">
                        <tbody>
                        {#each current_room.messages as message }
                            <tr>
                                <td>{message.user_name}: {message.text}</td>
                            </tr>
                        {/each}
                        </tbody>
                    </table>
                </div>
                <div class="chat-room-users">
                    <table class="strip">
                        <tbody>
                        {#each current_room.users as user (user.Id)}
                            <tr>
                                <td>{user.UserName}</td>
                            </tr>
                        {/each}
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="chat-room-footer">
                <div class="chat-room-input">
                    <TextInput
                        bind:input_text={message_to_send}
                        id="inMessageToSend"
                        label_text="Message">
                    </TextInput>
                </div>
                <div class="clear-button">
                    <Button button_label="X" on:click={clear_button_handler}></Button>
                </div>
                <div class="send-button">
                    <Button button_label="Send" on:click={send_button_handler}></Button>
                </div>
            </div>
        </div>
    </div>
</div>
<svelte:window on:keydown={keydown_handler}></svelte:window>

<style>
    .chat-container {
        display: flex;
        flex-direction: row;
        height: 100vh;
    }
    .minimized-sidenav {
        flex: 0 1 0;
        height: 100vh;
    }
    .spa-container {
        flex: 10 1 0;
        height: 100vh;
        margin: 0;
    }
    div.chat-list-header {
        display: flex;
        flex-direction: row;
        border-bottom: thin solid black;
        height: 15%;
        width: 100%;
        align-items:center;
    }
    h2.chat-list-title {
        flex: 8 0 0;
        font-size: large;
        padding-left: 2.5%;
    }
    div.chat-list-button {
        flex: 0 1 1;
        margin: 15px;
    }
    .chat-room-list-table {
        overflow-y: scroll;
        height: 85%;
        max-height: 85%;
        background-color: white;
    }
    .joined-chat-room {
        height: 100vh;
        display: flex;
        flex-direction: column;
    }
    .chat-room-header {
        display: flex;
        flex-direction: row;
        flex: 0 1 0;
    }
    .chat-room-header > h2 {
        flex: 8 1 0;
        font-size: large;
        padding-left: 2.5%;
    }
    .chat-room-header > .button {
        flex: 3 1 1;
        align-self: center;
        margin: 15px;
    }
    .chat-room-messages-container {
        display: flex;
        flex-direction: row;
        flex: 1 0 0;
        border: thin solid black;
        height: 80%;
        margin-left: 5px;
        background-color: white;
    }
    .chat-room-messages {
        flex: 1 1 0;
        overflow-y: scroll;
        height: 100%;
        max-height: 100%;
        display: flex;
        flex-direction: column-reverse;
        background-color: white;
    }
    .chat-room-users {
        flex: 0 0 25%;
        overflow-y: scroll;
        height: 100%;
        max-height: 100%;
        margin-left: 0.5%;
        padding-left: 0.5%;
        border-left: thin solid black;
    }
    .chat-room-footer {
        display: flex;
        flex-direction: row;
        flex: 0 1 0;
    }
    .chat-room-input {
        margin-left:5px;
        flex: 1 1 0;
    }
    .clear-button {
        align-self: center;
    }

    .send-button {
        align-self: center;
        flex: 0 0 0;
        margin: 5px;
        border-radius: 20%;
    }
    .sidenav {
        height: 100vh;
    }
    .sidenav-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        height: 100%;
    }
    .sidenav-logo {
        margin-top: 10%;
        flex: 0 1 auto;
        width: 90%;
    }
    .sidenav-filler {
        flex: 10 0 0;
    }
    .sidenav-btn {
        flex: 0 1 0;
        width: 80%;
    }
</style>
