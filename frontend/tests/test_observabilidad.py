import json
import logging
from flask import Flask
from observabilidad import configure_observability


def test_final_request_logs_and_correlation_exclude_user_input(caplog):
    app=Flask('observability-test');configure_observability(app)
    @app.post('/check')
    def check():return {'status':'unavailable'},503
    with caplog.at_level(logging.INFO,logger=app.logger.name):
        response=app.test_client().post('/check?token=private-query',data='private-body',headers={'X-Correlation-ID':'caller-controlled','Authorization':'Bearer private-authorization'})
    correlation=response.headers['X-Correlation-ID']
    assert len(correlation)==32 and correlation!='caller-controlled'
    record=json.loads(caplog.records[-1].message)
    assert record['status']==503 and record['endpoint']=='check' and record['correlation_id']==correlation
    assert all(value not in caplog.text for value in ('private-query','private-body','private-authorization'))
