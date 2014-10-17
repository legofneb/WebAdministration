angular.module("computer")
.config(function ($routeProvider, rootUrl) {
  $routeProvider.when('/', {
    templateUrl: rootUrl + "NG/Home"
  });

  $routeProvider.when('/Actions', {
    templateUrl: rootUrl + "NG/Actions"
  });

  $routeProvider.when('/IP', {
    templateUrl: rootUrl + "NG/IP"
  });

  $routeProvider.when('/Admin', {
    templateUrl: rootUrl + "NG/Admin"
  });

  $routeProvider.when('/EstimateDiskUsage', {
    templateUrl: rootUrl + "NG/EstimateDiskUsage"
  });

  $routeProvider.when('/TSM', {
    templateUrl: rootUrl + "NG/TSM"
  });

  $routeProvider.when('/USMT', {
    templateUrl: rootUrl + "NG/USMT"
  });

  $routeProvider.when('/WordPerfect', {
    templateUrl: rootUrl + "NG/WordPerfect"
  });

  $routeProvider.when('/MapDrive', {
    templateUrl: rootUrl + "NG/MapDrive"
  });

  $routeProvider.when('/IDF', {
    templateUrl: rootUrl + "NG/IDF"
  });

  $routeProvider.when('/USMTConsole', {
    templateUrl: rootUrl + "NG/USMTConsole"
  });

  $routeProvider.otherwise({
    redirectTo: '/'
  });
})
.controller("computerCtrl", function ($location, $http, $timeout, rootUrl, TSMUpdate, USMTUpdate, WordPerfectUpdate, IDFUpdate) {
  var self = this;

  self.computerSearchValue = "";
  self.searchForComputer = searchForComputer;
  self.addIPToComputer = addIPToComputer;
  self.setIPForComputer = setIPForComputer;

  // Actions
  self.admin = admin;
  self.estimateDiskUsage = estimateDiskUsage;
  self.TSM = TSM;
  self.startTSM = startTSM;
  self.USMTGo = USMTGo;
  self.startUSMT = startUSMT;
  self.wordPerfect = wordPerfect;
  self.checkWordPerfect = checkWordPerfect;
  self.installWordPerfect = installWordPerfect;
  self.IDF = IDF;
  self.installIDF = installIDF;

  self.backToActions = backToActions;
  self.backToHome = backToHome;

  self.adminDelete = adminDelete;
  self.adminAdd = adminAdd;
  self.getUSMTComputers = getUSMTComputers;

  $timeout(self.getUSMTComputers, 5000);
  self.usmt = USMTUpdate;
  self.USMTConsole = USMTConsole;
  self.IDFUpdate = IDFUpdate;

  self.setUSMTComputer = setUSMTComputer;

  function getUSMTComputers() {
    $http.get(rootUrl + 'API/USMT').success(function (data) {
      self.usmtComputers = data;
    });

    $timeout(self.getUSMTComputers, 20000);
  }

  function setUSMTComputer(IPAddress) {
    self.usmtCurrentIP = IPAddress;
  }

  function searchForComputer() {
    $http.get(rootUrl + 'API/Computer/' + self.computerSearchValue).success(function (data) {
      self.computer = data;
      $location.path('/Actions');
      console.log(data);
    });
  }

  function addIPToComputer() {
    $location.path('/IP');
    self.IPAddress = self.computer.IPAddress;
  }

  function setIPForComputer() {
    self.computer.IPAddress = self.IPAddress;
    $location.path('/Actions');
  }

  // Action Functions

  function estimateDiskUsage() {

    $location.path('/EstimateDiskUsage');
    if (!self.computer.diskUsage) {
      $http.post(rootUrl + 'API/EstimateDiskUsage', self.computer).success(function (data) {
        self.computer.diskUsage = data;
      });
    }
  }

  function admin() {
    $location.path('/Admin');
    $http.post(rootUrl + 'API/Admin', self.computer).success(function (data) {
      console.log(data);
      self.computer.admins = data;
    });
  }

  function TSM() {
    $location.path('/TSM');
    self.tsm = TSMUpdate;
  }

  function startTSM() {
    self.tsm.messages.push("Starting TSM...");
    $http.post(rootUrl + 'API/TSM', self.computer);
  }

  function USMTGo() {
    $location.path('/USMT');
  }

  function startUSMT() {
    self.usmt.messages[self.computer.IPAddress] = [];
    self.usmt.messages[self.computer.IPAddress].push("Starting USMT...");
    $http.post(rootUrl + 'API/USMT', self.computer);
  }

  function adminDelete(admin) {
    $http.put(rootUrl + 'API/Admin/' + encodeURIComponent(admin) + "?action=delete", self.computer).success(function (data) {
      self.admin();
    });
  }

  function adminAdd(admin) {
    $http.put(rootUrl + 'API/Admin/' + encodeURIComponent(admin) + "?action=add", self.computer).success(function (data) {
      self.admin();
    });
  }

  function wordPerfect() {
    $location.path('/WordPerfect');
    self.wordPerfectUpdate = WordPerfectUpdate;
  }

  function checkWordPerfect() {
    self.wordPerfectUpdate.messages.push("Checking...");
    $http.get(rootUrl + 'API/WordPerfect/' + self.computer.IPAddress).success(function (data) {
      if (data == 'true') {
        self.wordPerfectUpdate.messages.push("Word Perfect is installed");
      } else {
        self.wordPerfectUpdate.messages.push("Word Perfect is not installed");
      }
    });
  }

  function IDF() {
    $location.path('/IDF');
  }

  function installIDF() {
    self.IDFUpdate.messages[self.computer.IPAddress] = [];
    self.IDFUpdate.messages[self.computer.IPAddress].push("Starting IDF Install...");
    $http.post(rootUrl + 'API/IDF/', self.computer).success(function (data) {
      self.IDFUpdate.messages.push("Success!");
    });
  }

  function installWordPerfect() {
    $http.post(rootUrl + 'API/WordPerfect/', self.computer).success(function (data) {
      self.wordPerfectUpdate.messages.push("Started installation...check the status in a couple minutes.");
    });
  }

  function backToActions() {
    $location.path('/Actions');
  }

  function backToHome() {
    $location.path('/');
  }

  function USMTConsole() {
    $location.path('USMTConsole');
  }

})

.factory('TSMUpdate', ['$rootScope', 'Hub', function ($rootScope, Hub) {
  var self = this;
  self.messages = [];

  //declaring the hub connection
  var hub = new Hub('TSMHub', {

    //client side methods
    listeners: {
      'updateStatus': function (data) {
        self.messages.push(data);
        $rootScope.$apply();
      }
    }
  });

  return self;
}])

.factory('USMTUpdate', ['$rootScope', 'Hub', function ($rootScope, Hub) {
  var self = this;
  self.messages = {};

  //declaring the hub connection
  var hub = new Hub('USMTHub', {

    //client side methods
    listeners: {
      'updateStatus': function (data, id) {
        self.messages[id].push(data);
        $rootScope.$apply();
      }
    }
  });

  return self;
}])

.factory('WordPerfectUpdate', ['$rootScope', 'Hub', function ($rootScope, Hub) {
  var self = this;
  self.messages = [];

  //declaring the hub connection
  var hub = new Hub('WordPerfectHub', {

    //client side methods
    listeners: {
      'updateStatus': function (data) {
        self.messages.push(data);
        $rootScope.$apply();
      }
    }
  });

  return self;
}])

.factory('IDFUpdate', ['$rootScope', 'Hub', function ($rootScope, Hub) {
  var self = this;
  self.messages = [];

  //declaring the hub connection
  var hub = new Hub('IDFHub', {

    //client side methods
    listeners: {
      'updateStatus': function (data, id) {
        self.messages[id].push(data);
        $rootScope.$apply();
      }
    }
  });

  return self;
}])

.directive('ngConfirmClick', [
        function () {
          return {
            link: function (scope, element, attr) {
              var msg = attr.ngConfirmClick || "Are you sure?";
              var clickAction = attr.confirmedClick;
              element.bind('click', function (event) {
                if (window.confirm(msg)) {
                  scope.$eval(clickAction);
                }
              });
            }
          };
        }]);