import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 1,
    duration: '20s',
    thresholds: {
        http_req_duration: ['p(95)<2']
    }
}

const BASE_URL = `http://localhost:5148`;

export default () => {
    const courses = http.get(`${BASE_URL}/courses`).json()
    check(courses, { 'retrieved courses': (obj) => courses.length === 3 })
    sleep(1)
}
