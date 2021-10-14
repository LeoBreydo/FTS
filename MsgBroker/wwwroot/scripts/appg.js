var GlobalApp = GlobalApp || {};

var connection;
const max_num_messages = 100;

var msgapp = new Vue({
    el: '#msgapp',
    data: {
        items: []
    }
});

var curapp = new Vue({
    el: '#curapp',
    data: {
        items: []
    }
});

var grapp = new Vue({
    el: '#grapp',
    data: {
        userStyle: { backgroundColor: "green" },
        schedStyle: { backgroundColor: "black" },
        lossStyle: { backgroundColor: "red" },
        parStyle: { backgroundColor: "green" },
        contrStyle: { backgroundColor: "black" },
        sessStyle: { backgroundColor: "red" },
        errStyle: { backgroundColor: "green" },
        mktStyle: { backgroundColor: "black" },
        error_nbr: 0,
        Id: 0,
        connected: false
    }
});

var exapp = new Vue({
    el: '#exapp',
    data: {
       items:[]
    }
});

GlobalApp.start = function () {
    connection = new window.signalR.HubConnectionBuilder()
        .withUrl("/dataHub")
        .build();

    connection.on('updateStates', (sender, message) => {
        var state_object = JSON.parse(message);
        this.set_msgapp_data(state_object);
        this.set_grapp_data(state_object);
        this.set_curapp_data(state_object);
        this.set_exapp_data(state_object);
    });

    $(window).on("beforeunload",
        function() { connection.invoke('leaveAccount', window.accountId); });

    connection.start()
        .then(() => connection.invoke('joinAccount', window.accountId));
}

GlobalApp.set_grapp_data = function (state_object) {
    let objKeys = Object.keys(state_object.RestrictionDetails);
    objKeys.forEach(key => {
        let value = state_object.RestrictionDetails[key];
        var v = "green";
        if (value === 1) v = "red";
        else if (value === 2) v = "black";
        grapp[key].backgroundColor = v;
    });
    grapp.error_nbr = state_object.DayErrorNbr;
    grapp.Id = state_object.Id;
    grapp.connected = state_object.IsConnected;
}

GlobalApp.set_exapp_data = function (state_object) {
    exapp.items = [];
    state_object.ExSummaries.forEach(exs => {
        let objKeys = Object.keys(exs.RestrictionDetails);
        var summary = {
            Id: exs.Id,
            Name: exs.Name,
            Currency: exs.Currency,
            UPL: exs.UPL,
            RPL: exs.RPL
        };
        objKeys.forEach(key => {
            let value = exs.RestrictionDetails[key];
            var v = "green";
            if (value === 1) v = "red";
            else if (value === 2) v = "black";
            summary[key] = { backgroundColor: v };
        });
        exapp.items.push(summary);
    });
}

GlobalApp.set_curapp_data = function (state_object) {
    curapp.items = [];
    state_object.CGSummaries.forEach(cgs => {
        var summary = {
            Currency: cgs.Currency,
            UPL: cgs.UPL,
            RPL: cgs.RPL
        };
        curapp.items.push(summary);
    });
}

GlobalApp.set_msgapp_data = function (state_object) {
    var second_array = state_object.MessagesToShow;
    second_array.reverse();
    var merged = second_array.concat(msgapp.items);
    var length = merged.length;
    if (length > max_num_messages) { merged = merged.slice(0, max_num_messages); }
    msgapp.items = merged;
};

GlobalApp.DoPost = function (exchangeName) {
    $.post("/Home/Show/", { exName: exchangeName, ut: window.userTicket, selector: "Exchange" },
        function (data) {
            var w = window.open('about:blank');
            w.document.open();
            w.document.write(data);
            w.document.close();
        });
}

GlobalApp.Message = function (source_id, command_id, source_type) {
    connection.invoke('postUserCommand', [source_id, command_id, source_type]);
}



