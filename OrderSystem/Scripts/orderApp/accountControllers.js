/* global app */

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
		$rootScope.backBtn = false;
		$rootScope.indexBtn = true;
		$rootScope.closeBtn = false;

		$scope.codeImgSrc = '/Account/CodeImage'
		$scope.changeCodeImg = function () {
			$scope.codeImgSrc = '/Account/CodeImage?' + Math.random();
		}

		$scope.signinFormData = {};
		$scope.signin = function () {
			$http.post('/Account/Signin', $scope.signinFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					$location.path('/history');
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
		$rootScope.backBtn = false;
		$rootScope.indexBtn = true;
		$rootScope.closeBtn = false;

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
			if ($scope.signupFormData.Passwordaga != $scope.signupFormData.Password) {
				$scope.signupFormData.Passwordaga = '';
				alert('两次密码不匹配');
				return;
			}
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
	'$location',
	'$routeParams',
	function ($scope, $rootScope, $http, $location,$param) {
		$rootScope.viewTitle = '当前点单';
		$rootScope.backBtn = false;
		$rootScope.indexBtn = true;
		$rootScope.closeBtn = false;

		$http.post('/Cart/GetSavedMenu', {
			qrCode: $location.search().qrCode
		}).success(function (data) {
			console.log(data)
			$scope.historyMenu = data;
		});
		
		$http.post('/Cart/GetTablewareFee').success(function (data) {
			$scope.tablewareFee = parseFloat(data.TablewareFee);
		});

		$scope.tryAgain = function () {
			$rootScope.historyCart = {
				Customer: $scope.historyMenu.Customer,
				Results: $scope.historyMenu.Results
			}
			$location.path('/cart');
		}
	}
]).controller('historyCtrl', [
	'$scope',
	'$rootScope',
	'$http',
	'$location',
	'$routeParams',
	function ($scope, $rootScope, $http, $location,$param) {
		$rootScope.viewTitle = '历史点单';
		$rootScope.backBtn = false;
		$rootScope.indexBtn = true;
		$rootScope.closeBtn = false;

		var activeInfo;
		$http.post('/Cart/GetHistoryDineInfo').success(function (data) {
			if(data.length == 0){
				$scope.isNULL = true;
				return;
			}
			for (var i = 0; i < data.length; i++) {
				data[i].Additional = {
					IsSelected: false
				}
			}
			$scope.historyInfo = data;
			activeInfo = data[0];
			$scope.toggleSelected(activeInfo);
		});
		$http.post('/Cart/GetTablewareFee').success(function (data) {
			$scope.tablewareFee = parseFloat(data.TablewareFee);
		});


		$scope.toggleSelected = function (info) {
			activeInfo.Additional.IsSelected = false;
			info.Additional.IsSelected = true;
			activeInfo = info;
			$http.post('/Cart/GetHistoryMenu', {
				CheckID: info.CheckID
			}).success(function (data) {
				$scope.historyMenu = data;
			});
		}
		

		$scope.tryAgain = function () {
			$rootScope.historyCart = {
				Customer: $scope.historyMenu.Customer,
				Results: $scope.historyMenu.Results
			}
			$location.path('/cart');
		}
	}
]);


app.controller('privilegeCtrl', function () {

});
app.controller('forgetCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	'$interval',
	function ($scope, $rootScope, $location, $http, $interval) {
		$rootScope.viewTitle = '忘记密码';
		$rootScope.backBtn = false;
		$rootScope.indexBtn = true;
		$rootScope.closeBtn = false;

		$scope.signupFormData = {};
		$scope.isSendSMS = false;
		$scope.canSendSMS = true;
		$scope.sendSMSBtnText = '发送验证码';

		$scope.sendSMS = function () {
			$http.post('/Account/SendForgetSMS', {
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

		$scope.forget = function () {
			if ($scope.signupFormData.Passwordaga != $scope.signupFormData.Password) {
				$scope.signupFormData.Passwordaga = '';
				alert('两次密码不匹配');
				return;
			}
			$http.post('/Account/Forget', $scope.signupFormData).success(function (data) {
				if (data.IsSucceed) {
					$rootScope.refreshClient();
					$location.path('/signin');
				} else {
					alert(data.ErrorMessage);
				}
			});
		}
	}
]);