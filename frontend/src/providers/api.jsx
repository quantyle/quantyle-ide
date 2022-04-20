import axios from 'axios';

const DEBUG = true;
const apiHost = DEBUG ? 'http://localhost:8001' : 'http://localhost:8000';

const apiHost2 = 'http://localhost:5000';

class Api {
    constructor(token = '') {
        this.token = token || localStorage.getItem('token');
    }

    aquireToken(email, password) {
        let self = this;
        let requestPromise = axios({
            method: 'post',
            url: apiHost + '/api/auth/',
            data: {
                username: email,
                password: password,
            }
        });
        requestPromise.then(resp => {
            self.setToken(resp.data.token);
        });
        return requestPromise;
    }

    createAccount(email, password, firstName, lastName, firstTimeLogin) {
        return axios({
            method: 'post',
            url: apiHost + '/api/new-user/',
            data: {
                email: email,
                password: password,
                first_name: firstName,
                last_name: lastName,
                first_time_login: firstTimeLogin
            }
        });
    }

    resetPassword(email) {
        return axios({
            method: 'post',
            url: apiHost + '/api/user/resetPassword/',
            //url: this.getPrefixURL() + '/api/user/resetPassword/',
            data: {
                email: email
            }
        });
    }

    setToken(token) {
        console.log('Token set...');
        console.log(token);
        localStorage.setItem('token', token);
        this.token = token;
    }

    clearToken() {
        localStorage.removeItem('token');
        this.token = '';
    }

    hasToken() {
        return this.token ? (true) : (false);
    }

    setCache(value) {
        let value_string = JSON.stringify(value);
        localStorage.setItem('cache', value_string);
    }

    // getCache() {
    //     let value_string = localStorage.getItem('cache');
    //     let json_value = JSON.parse(value_string);
    //     return json_value;
    // }

    clearCache() {
        localStorage.removeItem('cache');
    }


    post(endpoint = '/', data = {}) {
        return axios({
            method: 'post',
            url: apiHost + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            data: data,
        });
    }

    put(endpoint = '/', data = {}) {
        return axios({
            method: 'put',
            url: apiHost + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            data: data,
        });
    }

    get(endpoint = '/', params = {}) {
        return axios({
            method: 'get',
            url: apiHost + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
        });
    }

    delete(endpoint = '/', params = {}) {
        return axios({
            method: 'delete',
            url: apiHost + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
        });
    }

    getCache() {
        // var params = { exchange, product, interval, start, end };
        var endpoint = '/memCache';
        return axios({
            method: 'get',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            //params: params,
        });
    }

    getIndicators() {
        return this.indicators;
    }

    getExchanges() {
        return this.get('/media/exchanges.json');
    }

    // getChartData(exchange, product, interval, start, end) {
    //     return this.get('/api/chart/', { exchange, product, interval, start, end });
    // }

    getChartData(exchange, product, interval, start, end) {
        // var params = { exchange, product, interval, start, end };
        var endpoint = '/chart/' + exchange + '/' + product;
        return axios({
            method: 'get',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            //params: params,
        });
    }


    postPython(params) {
        // var params = { exchange, product, interval, start, end };
        var endpoint = '/python/';
        return axios({
            method: 'post',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
        });
    }

    getPortfolio() {
        // var params = { exchange, product, interval, start, end };
        var endpoint = '/portfolio';
        return axios({
            method: 'get',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            //params: params,
        });
    }


    getCode(params) {
        // gets source code
        var endpoint = '/code';
        return axios({
            method: 'get',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
            //params: params,
        });
    }

    postCode(params) {
        // saves source code
        var endpoint = '/code';
        return axios({
            method: 'post',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
            //params: params,
        });
    }

    getFiles(params) {
        // gets a list of files
        var endpoint = '/file';
        return axios({
            method: 'get',
            url: apiHost2 + endpoint,
            headers: { 'Authorization': 'Token ' + this.token },
            params: params,
            //params: params,
        });
    }


    getAccounts() {
        return this.get('/api/accounts/', { type: 'get_accounts' });
    }

    getAnAccount(account_id) {
        return this.get('/api/accounts/', { type: 'get_an_account', account_id });
    }

    getAccountHistory(account_id) {
        return this.get('/api/accounts/', { type: 'get_account_history', account_id });
    }

    listOrders(product_id) {
        return this.get('/api/orders/', { request_id: 'list_orders', product_id })
    }

    listFills(product_id) {
        return this.get('/api/orders/', { request_id: 'list_fills', product_id })
    }



    placeOrder(product_id, side, type, price, size) {
        return this.get('/api/orders/', {
            product_id: product_id,
            side: side,
            type: type,
            price: price,
            size: size,
        });
    }


    // cancelOrder(
    //     product_id,
    //     order_id) {
    //     return this.delete('/api/orders/', {
    //         request_id: 'cancel_order',
    //         product_id,
    //         side,
    //         type,
    //         price,
    //         size,
    //         stop,
    //         stop_price
    //     });
    // }

    cancelAllOrders(
        product_id,
        order_id) {
        return this.delete('/api/orders/', {
            request_id: 'cancel_all',
            product_id,
            order_id,
        });
    }

}

var api = new Api();

export default api;