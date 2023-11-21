<script>
    let websocket = undefined;
    
    export let user_id = 0;
    export let name = "";
    export let username = "";

    let create_room_callback = undefined;
    let join_room_callback = undefined;
    let leave_room_callback = undefined;
    let send_message_callback = undefined;
    let receive_message_callback = undefined;
    let user_joined_callback = undefined;
    let user_left_callback = undefined;

    let close_callback = undefined;
    
    export const Connect = function (session_token, success_callback, failure_callback, close_callback_) {
        let get_user_request = new XMLHttpRequest();
        get_user_request.open("GET", '/user');
        get_user_request.onloadend = function (e) {
            switch (get_user_request.status) {
                case 200:
                    let userData = JSON.parse(get_user_request.response);
                    user_id = userData.id;
                    name = userData.name;
                    username = userData.username;
                    ConnectWebsocket(success_callback, failure_callback, close_callback_);
                    break;
                default:
                    if(failure_callback !== undefined && failure_callback !== null)
                        failure_callback();
                    break;
            }
        };
        get_user_request.setRequestHeader("Authorization", 'Bearer ' + session_token);
        get_user_request.send();
    };

    const ConnectWebsocket = function (success_callback, failure_callback, close_callback_) {
        close_callback = close_callback_;
        
        let sessionToken = localStorage.getItem("sessionToken");
        websocket = new WebSocket(`ws://${window.location.host}/chat-ws`, sessionToken);
        websocket.addEventListener("open", (event) => {
            if(success_callback !== undefined && success_callback !== null)
                success_callback();
        });
        websocket.addEventListener("error", (event) => {
            if(failure_callback !== undefined && failure_callback !== null)
                failure_callback();
        });
        websocket.addEventListener("message", MessageHandler);
        websocket.addEventListener("close", (event) => {
            if(close_callback !== undefined && close_callback !== null)
                close_callback();
        });
    };
    
    let list_callback = undefined;
    const MessageHandler = function(message) {
        let response = JSON.parse(message.data);
        if(!response.IsSuccessful) {
            switch (response.Type) {
                case "ResponseCreateChatRoom":
                case "ResponseJoinChatRoom":
                case "ResponseListChatRoom":
                case "ResponseLeaveChatRoom":
                case "ResponseSendChatRoomMessage":
                case "ReceiveChatRoomMessage":
                case "UserJoinedChatRoom":
                case "UserLeftChatRoom":
                    break;
            }
            return;
        }

        switch (response.Type) {
            case "ResponseCreateChatRoom":
                if(create_room_callback !== undefined && create_room_callback !== null)
                    create_room_callback(response.Data);
                break;
            case "ResponseListChatRoom":
                if(list_callback !== undefined && list_callback !== null)
                    list_callback(response.Data.ChatRooms);
                break;
            case "ResponseJoinChatRoom":
                if(join_room_callback !== undefined && join_room_callback !== null)
                    join_room_callback(response.Data);
                break;
            case "ResponseLeaveChatRoom":
                if(leave_room_callback !== undefined && leave_room_callback !== null)
                    leave_room_callback(response.Data);
                break;
            case "ResponseSendChatRoomMessage":
                if(send_message_callback !== undefined && send_message_callback !== null)
                    send_message_callback(response.Data);
                break;
            case "ReceiveChatRoomMessage":
                if(receive_message_callback !== undefined && receive_message_callback !== null)
                    receive_message_callback(response.Data);
                break;
            case "UserJoinedChatRoom":
                if(user_joined_callback !== undefined && user_joined_callback !== null)
                    user_joined_callback(response.Data);
                break;
            case "UserLeftChatRoom":
                if(user_left_callback !== undefined && user_left_callback !== null)
                    user_left_callback(response.Data);
                break;
        }
    };
    
    export const CreateChatRoom = function (room_name, create_callback, message_callback, join_callback, leave_callback) {
        create_room_callback = create_callback;
        receive_message_callback = message_callback;
        user_joined_callback = join_callback;
        user_left_callback = leave_callback;

        websocket.send(
            JSON.stringify(
                {
                    Type: "CreateChatRoom",
                    Data: { Name: room_name }
                }));
    };
    export const ListChatRoom = function (callback) {
        websocket.send(
            JSON.stringify(
                {
                    Type: "ListChatRoom",
                    Data: {}
                }));
        
        list_callback = callback;
    };
    export const JoinChatRoom = function (id, join_callback, message_callback, user_join_callback, leave_callback) {
        join_room_callback = join_callback;
        receive_message_callback = message_callback;
        user_joined_callback = user_join_callback;
        user_left_callback = leave_callback;

        websocket.send(
            JSON.stringify(
                {
                    Type: "JoinChatRoom",
                    Data: {
                        Id: id,
                    }
                }));
    };
    export const LeaveChatRoom = function (leave_callback) {
        leave_room_callback = leave_callback;
        
        websocket.send(
            JSON.stringify(
                {
                    Type: "LeaveChatRoom",
                    Data: {}
                }
            )
        );
    };
    export const SendChatRoomMessage = function (message, send_callback) {
        send_message_callback = send_callback;

        websocket.send(
            JSON.stringify(
                {
                    Type: "SendChatRoomMessage",
                    Data: { Message: message }
                }
            )
        );
    }
</script>
