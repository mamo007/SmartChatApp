"use strict";

let currLoggedUser = document.getElementById("currLoggedUser").value;

function checkCurrUser(user) {
    if (currLoggedUser == user) return true;
    return false;
}

function addDeleteIcon(user, msgid, msg, read, softdelete) {
    if (checkCurrUser(user))
    {
        var deleteModal = "deleteModal-" + msgid;
        var deleteModalTag = "#deleteModal-" + msgid;
        var deleteModalLabel = "deleteModalLabel-" + msgid;
        var deleteButton = "deleteButton-" + msgid;
        return `
            <button type="button" class="btn btn-outline-primary btn-sm float-end my-2" style="font-size:12px" data-bs-toggle="modal" data-bs-target="${deleteModalTag}">
                <span class="bi bi-trash" ></span>
            </button>

            <div class="modal fade text-dark" id="${deleteModal}" tabindex="-1" aria-labelledby="${deleteModalLabel}" aria-hidden="true">
              <div class="modal-dialog">
                <div class="modal-content">
                  <div class="modal-header">
                    <h5 class="modal-title" id="${deleteModalLabel}">Are you sure you want to delete this?</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                  </div>
                  <div class="modal-body text-center">
                    ${msg}
                  </div>
                  <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button id="${deleteButton}" data-bs-dismiss="modal" type="button" class="btn btn-danger">Delete</button>
                  </div>
                </div>
              </div>
            </div>
        `;
    }
    else
        return '';
}

function liEncodeMsg(date, user, msg, messageid, read, softdelete) {
    if (softdelete == 0) {
        if (checkCurrUser(user)) {
            return `
            <div class="float-end">
                
                <p class="text-center bg-primary text-light rounded-pill px-2 py-1 m-1">${msg}</p>
                <label class="float-end mx-1" style="font-size:12px;">${date}</label>
            </div>
            ${addDeleteIcon(user, "m" + messageid, msg)}
        `;
        }
        else {
            return `
            <div class="float-start">
                <p class="text-center bg-secondary text-light rounded-pill px-2 py-1 m-1">${msg}</p>
                <label class="float-start mx-1" style="font-size:12px;">${date}</label>
            </div>
        `;
        }
    }
    else if (softdelete == 1) {
        if (checkCurrUser(user)) {
            return `
            <div class="float-end">
                
                <p class="text-center bg-primary text-light rounded-pill px-2 py-1 m-1"><i><small>Message was deleted!</small></i></p>
                <label class="float-end mx-1" style="font-size:12px;">${date}</label>
            </div>
        `;
        }
        else {
            return `
            <div class="float-start">
                <p class="text-center bg-secondary text-light rounded-pill px-2 py-1 m-1"><i><small>Message was deleted!</small></i></p>
                <label class="float-start mx-1" style="font-size:12px;">${date}</label>
            </div>
        `;
        }
    }
}

// Create connection to hub
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").withAutomaticReconnect().build();

// Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

// Handle Receive message
connection.on("ReceiveMessage", function (date, user, message, messageid, read, softdelete) {

    if (softdelete < 2) {
        // Styling magic
        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\n/g, '<br/>');
        var encodedMsg = liEncodeMsg(date, user, msg, messageid, read, softdelete);
        var li = document.createElement("li");
        li.innerHTML = encodedMsg;
        li.className = 'clearfix';
        li.id = "m" + messageid;

        let msglist = document.getElementById("messagesList");
        msglist.appendChild(li);

        //Scroll to last message
        msglist.scrollTop = msglist.scrollHeight;
        document.getElementsByClassName("emoji-wysiwyg-editor")[0].focus();
        document.getElementById('messageInput').focus();

        //Handle delete click
        let messagesList = msglist.getElementsByTagName('li');
        for (let i = 0; i < messagesList.length; i++) {
            if (messagesList[i].children[2] != null) {
                let btnid = messagesList[i].children[2].children[0].children[0].children[2].children[1].id;
                document.getElementById(btnid).addEventListener('click', function (e) {
                    connection.invoke("SoftDeleteMessages", btnid).catch(function (err) {
                        return console.error(err.toString());
                    }).then(() => {

                    });
                    e.preventDefault();
                });
            }
        }
    }
});

//Handle delete message
connection.on("SoftDeleteMessage", function (id, date, user) {
    let msgbyid = document.getElementById("m" + id);
    if (checkCurrUser(user)) {
        msgbyid.innerHTML = `
            <div class="float-end">
                
                <p class="text-center bg-primary text-light rounded-pill px-2 py-1 m-1"><i><small>Message was deleted!</small></i></p>
                <label class="float-end mx-1" style="font-size:12px;">${date}</label>
            </div>
        `;
    }
    else {
        msgbyid.innerHTML = `
            <div class="float-start">
                <p class="text-center bg-secondary text-light rounded-pill px-2 py-1 m-1"><i><small>Message was deleted!</small></i></p>
                <label class="float-start mx-1" style="font-size:12px;">${date}</label>
            </div>
        `;
    }
});


connection.on("Online", function (user) {
    document.getElementById("Online"+user).classList.add("text-success");
});

connection.on("Offline", function (user) {
    document.getElementById("Online"+user).classList.remove("text-success");
});

// Start connection
connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    // Notice others we are here
    connection.invoke("Joined").catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("pubchat").addEventListener('click', function (e) {
        RemoveOkClass(e);
        document.getElementById("pubchat").classList.add("ok");
        document.getElementById("pubchat").classList.add("text-info");
        document.getElementById("messagesList").innerHTML = "";
        connection.invoke("Joined").catch(function (err) {
            return console.error(err.toString());
        });
    });
}).catch(function (err) {
    return console.error(err.toString());
});

// Handle send click
var sendFc = function (event) {
    var message = document.getElementsByClassName("emoji-wysiwyg-editor")[0].innerText;
    var CheckPubChatActive = document.getElementById("pubchat").classList.contains("ok");

    if (CheckPubChatActive) {
        connection.invoke("SendMessage", message).catch(function (err) {
            return console.error(err.toString());
        }).then(() => {
            // Clean text box
            document.getElementById("messageInput").value = "";
            document.getElementById("messageInput").nextSibling.textContent = "";
        });
    }
    else {
        var getUsernameByClass = document.getElementsByClassName("ok")[0].id;
        connection.invoke("SendMessage", "/pm: " + getUsernameByClass + " " + message).catch(function (err) {
            return console.error(err.toString());
        }).then(() => {
            // Clean text box
            document.getElementById("messageInput").value = "";
            document.getElementById("messageInput").nextSibling.textContent = "";
        });
    }

    event.preventDefault();
};
document.getElementById("sendButton").addEventListener("click", sendFc);

//Handle private chats
let chatsList = document.getElementById('chatsList').getElementsByTagName('li');
for (let i = 1; i < chatsList.length; i++) {
    document.getElementById(chatsList[i].children[1].id).addEventListener('click', function (e) {
        RemoveOkClass(e);
        document.getElementById(chatsList[i].children[1].id).classList.add("ok");
        document.getElementById(chatsList[i].children[1].id).classList.add("text-info");
        
        document.getElementById("messagesList").innerHTML = "";
        connection.invoke("PrivateMessages", chatsList[i].children[1].id).catch(function (err) {
            return console.error(err.toString());
        }).then(() => {
        });

    });
}
//delay 1 second to make sure emoji are loaded
setTimeout(function () {
    let emojimsginput = document.getElementsByClassName("emoji-wysiwyg-editor")[0];
    emojimsginput.addEventListener("keydown", function (e) {
        // Enter was pressed without shift key
        if (e.key === 'Enter' && !e.shiftKey) {
            // prevent default behavior
            e.preventDefault();
            sendFc(e);
        }
    });
}, 1000)


//add/remove ok class to know which chat is active
function RemoveOkClass(e) {
    var x = e.target.parentElement.parentElement.children;
    for (let i = 0; i < x.length; i++) { 
        document.getElementById(x[i].children[1].id).classList.remove("ok");
        document.getElementById(x[i].children[1].id).classList.remove("text-info");
    }
        
}