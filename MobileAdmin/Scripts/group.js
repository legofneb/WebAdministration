angular.module("groupApp")
.config(function ($routeProvider, rootUrl) {
  $routeProvider.when('/', {
    templateUrl: rootUrl + "OU/Home"
  });

  $routeProvider.when('/Actions', {
    templateUrl: rootUrl + "OU/Actions"
  });

  $routeProvider.otherwise({
    redirectTo: '/'
  });
})
.controller("groupCtrl", function ($location, $http, rootUrl) {
  var self = this;
  
  self.setContainer = setContainer;
  self.computerList = [];

  self.navigateToContainer = navigateToContainer;
  self.identityFinder = identityFinder;

  navigateToContainer('');

  function navigateToContainer(container) {
    $http.get(rootUrl + 'API/OU/' + container).success(function (data) {
      self.OUInfo = data;
    });
  }

  function setContainer(container) {
    self.container = container;
    $location.path('/Actions');
    $http.get(rootUrl + 'API/ComputersFromOU/' + container).success(function (data) {
      angular.forEach(data, function (value, key) {
        $http.get(rootUrl + 'API/Computer/' + value).success(function (data) {
          if (data.LeaseTag) {
            self.computerList.push(data);
          }
        });
      });
    });
  }

  function identityFinder() {

  }

})

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