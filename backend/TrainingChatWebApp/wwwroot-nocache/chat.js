const enumState = {
	ListRoom: 0,
	CreateRoom: 1,
	ChatRoom: 2
}

document.addEventListener('DOMContentLoaded', function () {
	var elems = document.querySelectorAll('.modal');
	var elemsSideNav = document.querySelectorAll('.sidenav');
	var instances = M.Modal.init(elems);
	var instancesSideNav = M.Sidenav.init(elemsSideNav);
	window.modalInstances = instances;
	window.sideNavInstances = instancesSideNav;
});
document.addEventListener("keydown", (event) => {
	if (!event.isComposing && event.key === "Enter") {
		switch (window.State) {
			case enumState.ListRoom:
				break;
			case enumState.CreateRoom:
				RequestCreateChatRoom();
				break;
			case enumState.ChatRoom:
				RequestSendChatRoomMessage();
				break;
			default:
				break;
		}		
	} else if (event.key === "Escape") {
		ClearMessage();
	}
});
function Initialize(a) {
	let sessionToken = localStorage.getItem("sessionToken");
	console.log(sessionToken);
    window.State = enumState.ListRoom;
	let getuserRequest = new XMLHttpRequest();
	getuserRequest.open("GET", '/user');
	getuserRequest.onloadend = function (e) {
		switch (getuserRequest.status) {
			case 200:
				let userData = JSON.parse(getuserRequest.response);
				let showUserName = document.getElementById("showUserName");
				showUserName.innerHTML = `Welcome, ${userData.username}`
				window.userData = userData;
				ChatConnection(userData);
				break;
			case 400:
			case 401:
				window.location.href = 'index.html'
				break;
			case 500:
				console.log("Our server is out");
				break;
		}
	};
	getuserRequest.setRequestHeader("Authorization", 'Bearer ' + sessionToken,);
	getuserRequest.send();
}

function OpenCreateModal() {
	setTimeout(function (){ document.getElementById("inRoomName").focus(); }, 250);
	window.State = enumState.CreateRoom
}

function ChatConnection(userData) {
	let sessionToken = localStorage.getItem("sessionToken");
	let websocket = new WebSocket(`ws://${window.location.host}/chat-ws`, sessionToken);
	window.websocket = websocket;
	websocket.addEventListener("open", (event) => {
		console.log('Conectou');
		RequestListChatRoom(websocket);
	});
	websocket.addEventListener("message", (event) => {
		console.log(event);
		var response = JSON.parse(event.data);
		console.log(response);

		switch (response.Type) {
			case "ResponseCreateChatRoom":
				ResponseCreateChatRoom(response.Data);
				console.log("ResponseCreateChatRoom");
				break;
			case "ResponseListChatRoom":
				console.log("ResponseListChatRoom");
				ResponseListChatRoom(response.Data.ChatRooms);
				break;
			case "ResponseJoinChatRoom":
				console.log("ResponseJoinChatRoom");
				ResponseJoinChatRoom(response.Data);
				break;
			case "ResponseLeaveChatRoom":
				ResponseLeaveChatRoom(response.Data);
				break;
			case "ReceiveChatRoomMessage":
				NotificationReceiveChatRoomMessage(response.Data);
				break;
			case "UserJoinedChatRoom":
				console.log("UserJoinedChatRoom");
				NotificationUserJoined(response.Data.UserName);
				break;
			case "UserLeftChatRoom":
				console.log("UserLeftChatRoom");
				NotificationUserLeftChatRoom(response.Data.UserName);
				break;
			case "ResponseSendChatRoomMessage":
				console.log("ResponseSendChatRoomMessage");
				break;
		}
	});
	websocket.addEventListener("close", (event) => {
		console.log("Fechou");
	});
}
function ResponseCreateChatRoom(data) {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");
	let nameRoom = document.getElementById("inRoomName");
	let chatNameRoom = document.getElementById("chatNameRoom");
	let outAlertRoom = document.getElementById("outAlertRoom");
	window.modalInstances.forEach(element => {
		if (element.id == "createRoomModal") {
			element.close();
		}
	});
	window.State = enumState.ChatRoom;
	outAlertRoom.innerHTML = "";
	chatRoomUsersElement.innerHTML = "";
	chatRoomListElement.classList.add("hide");
	joinedChatRoomElement.classList.remove("hide");
	nameRoom.value = "";
	chatNameRoom.innerHTML = data.Name;
	let newUserElement = chatRoomUsersElement.insertRow();
	newUserElement.innerHTML = window.userData.name;
	document.getElementById("inMessage").focus();
}
function ResponseLeaveChatRoom() {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");

	chatRoomMessagesElement.innerHTML = "";
	chatRoomUsersElement.innerHTML = "";

	chatRoomListElement.classList.remove("hide");
	joinedChatRoomElement.classList.add("hide");

	window.State = enumState.ListRoom;
	RequestListChatRoom(window.websocket);
}
function ResponseListChatRoom(roomArray) {
	let tableElement = document.getElementById("idChatRoomListTable");
	let tableBodyElement = tableElement.getElementsByTagName("tbody")[0];
	tableBodyElement.innerHTML = "";
	for (let i = 0; i < roomArray.length; i++) {
		console.log(roomArray[i]);
		let newElement = tableBodyElement.insertRow();
		newElement.innerHTML = roomArray[i].Name;
		newElement.onclick = function () { RequestJoinChatRoom(window.websocket, roomArray[i].Name, roomArray[i].Id) };
	}
}
function NotificationReceiveChatRoomMessage(Data) {
	let chatRoomMessageElement = document.getElementById("idChatRoomMessages");

	let newMessage = chatRoomMessageElement.insertRow();
	newMessage.innerHTML = `${Data.UserName} : ${Data.Message}`;
}

function ResponseJoinChatRoom(chatRoomData) {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");
	let chatNameRoom = document.getElementById("chatNameRoom");

	chatRoomListElement.classList.add("hide");
	joinedChatRoomElement.classList.remove("hide");

	chatRoomData.UserNames.forEach((userName) => {
		let newUserElement = chatRoomUsersElement.insertRow();
		newUserElement.id = userName;
		newUserElement.innerHTML = userName;
	});
	chatNameRoom.innerHTML = chatRoomData.Name;
	window.State = enumState.ChatRoom;
	document.getElementById("inMessage").focus();
}

function NotificationUserLeftChatRoom(userName) {
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");
	let tableBodyElement = chatRoomUsersElement.getElementsByTagName("tbody")[0];

	for (let i = 0; i < tableBodyElement.rows.length; i++) {
		console.log(tableBodyElement.rows[i].id);
		if (tableBodyElement.rows[i].id == userName) {
			tableBodyElement.deleteRow(i);
			break;
		}
	}
}

function NotificationUserJoined(userName) {
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");

	let newUser = chatRoomUsersElement.insertRow();
	newUser.id = userName;
	newUser.innerHTML = userName;
}

// Messages to server.

function RequestCreateChatRoom() {
	let nameRoom = document.getElementById("inRoomName").value;
	let outAlertRoom = document.getElementById("outAlertRoom");
	if (nameRoom.length == 0) {
		outAlertRoom.innerHTML = "The name can't be empty"
		return
	}
	websocket.send(
		JSON.stringify(
			{
				Type: "CreateChatRoom",
				Data: { Name: nameRoom }
			}));
	console.log(websocket);
	console.log("Sala " + nameRoom + " criada!");

}

function RequestSendChatRoomMessage() {
	let chatRoomMessageElement = document.getElementById("inMessage");
	let chatRoomMessage = document.getElementById("inMessage").value;

	if (chatRoomMessage.length == 0) { return }
	websocket.send(
		JSON.stringify(
			{
				Type: "SendChatRoomMessage",
				Data: { Message: chatRoomMessage }
			}
		)
	);
	chatRoomMessageElement.value = "";
}

function RequestLeaveChatRoom() {
	websocket.send(
		JSON.stringify(
			{
				Type: "LeaveChatRoom",
				Data: {}
			}
		)
	);
}

function RequestListChatRoom(websocket) {
	websocket.send(
		JSON.stringify(
			{
				Type: "ListChatRoom",
				Data: {}
			}));
}

function RequestJoinChatRoom(websocket, name, id) {
	websocket.send(
		JSON.stringify(
			{
				Type: "JoinChatRoom",
				Data: {
					Id: id,
				}
			}));
	console.log(websocket);
	console.log("cliquei no " + name + " / ID: " + id);
}

function ClearMessage() {
	let chatRoomMessageElement = document.getElementById("inMessage");
	chatRoomMessageElement.value = "";
}

function Logout() {
	localStorage.clear();
	window.location.href = 'index.html'
}
