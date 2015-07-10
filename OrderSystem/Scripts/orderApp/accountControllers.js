// Root
app.controller('accountCtrl', [
	'$scope',
	'$rootScope',
	'isAuthenticated',
	'$http',
	'$location',
	function ($scope, $rootScope, IAed, $http, $location) {
		$rootScope.refreshClient = function () {
			IAed().then(function (data) {
				if (data.IsSucceed) {
					$scope.isAuthenticated = true;
					$scope.clientName = data.Addition.Name;
				} else {
					$scope.isAuthenticated = false;
				}
			});
		}

		$scope.isAuthenticated = false;
		$rootScope.refreshClient();

		$scope.signout = function () {
			$http.post('/Account/Signout').success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					$location.path('/');
				}
			})
		}
	}
]);

app.controller('signinCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	function ($scope, $rootScope, $location, $http) {
		$rootScope.viewTitle = '登录';
		$rootScope.hideBackBtn = true;

		$scope.signinFormData = {};
		$scope.signin = function () {
			$http.post('/Account/Signin', $scope.signinFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					$location.path('/client');
				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]).controller('signupCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	'$interval',
	function ($scope, $rootScope, $location, $http, $interval) {
		$rootScope.viewTitle = '注册';
		$rootScope.hideBackBtn = true;

		$scope.signupFormData = {};
		$scope.isSendSMS = false;
		$scope.canSendSMS = true;
		$scope.sendSMSBtnText = '发送验证码';

		$scope.sendSMS = function () {
			$http.post('/Account/SendSMS', {
				Mobile: $scope.signupFormData.Mobile
			}).success(function (data) {
				if (data.IsSucceed) {
					$scope.isSendSMS = true;
					$scope.canSendSMS = false;

					var interval = 60;
					var timer = $interval(function () {
						$scope.sendSMSBtnText = interval + '秒后重发';
						interval--;
						if (interval == 0) {
							$scope.canSendSMS = true;
							$scope.sendSMSBtnText = '重发';
							interval = 60;
							$interval.cancel(timer);
						}
					}, 1000);
				} else {
					alert(data.ErrorMessage);
				}
			});
		}

		$scope.signup = function () {
			$http.post('/Account/Signup', $scope.signupFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					$location.path('/client');
				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]);

app.controller('clientCtrl', [
	'$scope',
	'$rootScope',
	'$http',
	function ($scope, $rootScope, $http) {
		$rootScope.viewTitle = '历史订单';
		$rootScope.hideBackBtn = true;


		$http.post('/Cart/GetSavedMenu').success(function (data) {
			console.log(data)
			$scope.historyMenu = data;
		});
	}
]);