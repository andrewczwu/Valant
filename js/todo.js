/// <reference path="jquery-1.7.2.js" />
/// <reference path="knockout-2.1.0.js" />

function ToDoViewModel() {
    var self = this;

    function ToDoItem(root, id, Label, finished) {
        var self = this,
            updating = false;

        self.id = id;
        self.Label = ko.observable(Label);
        self.finished = ko.observable(finished);

        self.remove = function () {
            root.sendDelete(self);
        };

        self.update = function (Label, finished) {
            updating = true;
            self.Label(Label);
            self.finished(finished);
            updating = false;
        };

        self.finished.subscribe(function () {
            if (!updating) {
                root.sendUpdate(self);
            }
        });
    };

    self.addItemLabel = ko.observable("");
    self.items = ko.observableArray();

    self.add = function (id, Label, finished) {
        self.items.push(new ToDoItem(self, id, Label, finished));
    };

    self.remove = function (id) {
        self.items.remove(function (item) { return item.id === id; });
    };

    self.update = function (id, Label, finished) {
        var oldItem = ko.utils.arrayFirst(self.items(), function (i) { return i.id === id; });
        if (oldItem) {
            oldItem.update(Label, finished);
        }
    };

    self.sendCreate = function () {
        $.ajax({
            url: "/api/item",
            data: { 'Label': self.addItemLabel(), 'Finished': false },
            type: "POST"
        });

        self.addItemLabel("");
    };

    self.sendDelete = function (item) {
        $.ajax({
            url: "/api/item/" + item.id,
            type: "DELETE"
        });
    }

    self.sendUpdate = function (item) {
        $.ajax({
            url: "/api/item/" + item.id,
            data: { 'Label': item.Label(), 'Finished': item.finished() },
            type: "PUT"
        });
    };
};

$(function () {
    var viewModel = new ToDoViewModel(),
        hub = $.connection.todo;

    ko.applyBindings(viewModel);

    hub.addItem = function (item) {
        viewModel.add(item.ID, item.Label, item.Finished);
    };
    hub.deleteItem = function (id) {
        viewModel.remove(id);
    };
    hub.updateItem = function (item) {
        viewModel.update(item.ID, item.Label, item.Finished);
    };

    $.connection.hub.start();

    $.get("/api/item", function (items) {
        $.each(items, function (idx, item) {
            viewModel.add(item.ID, item.Label, item.Finished);
        });
    }, "json");
});
