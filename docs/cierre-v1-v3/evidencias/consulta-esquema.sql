SELECT jsonb_build_object(
 'serverVersion',current_setting('server_version'),
 'columns',(SELECT jsonb_agg(to_jsonb(c) ORDER BY table_name,ordinal_position) FROM (SELECT table_name,column_name,data_type,is_nullable,character_maximum_length,ordinal_position FROM information_schema.columns WHERE table_schema='public') c),
 'constraints',(SELECT jsonb_agg(jsonb_build_object('table',c.conrelid::regclass::text,'name',c.conname,'kind',c.contype,'definition',pg_get_constraintdef(c.oid)) ORDER BY c.conname) FROM pg_constraint c JOIN pg_namespace n ON n.oid=c.connamespace WHERE n.nspname='public'),
 'indexes',(SELECT jsonb_agg(to_jsonb(i) ORDER BY tablename,indexname) FROM (SELECT tablename,indexname,indexdef FROM pg_indexes WHERE schemaname='public') i),
 'routines',(SELECT jsonb_agg(jsonb_build_object('name',p.proname,'kind',p.prokind,'signature',pg_get_function_identity_arguments(p.oid)) ORDER BY p.proname) FROM pg_proc p JOIN pg_namespace n ON n.oid=p.pronamespace WHERE n.nspname='public'),
 'triggers',(SELECT jsonb_agg(jsonb_build_object('table',tgrelid::regclass::text,'name',tgname,'definition',pg_get_triggerdef(oid)) ORDER BY tgname) FROM pg_trigger WHERE NOT tgisinternal AND tgrelid IN (SELECT oid FROM pg_class WHERE relnamespace='public'::regnamespace)),
 'foreignKeyColumns',(SELECT jsonb_agg(jsonb_build_object('table',cl.relname,'column',a.attname,'nullable',NOT a.attnotnull) ORDER BY cl.relname,a.attname) FROM pg_constraint c JOIN pg_class cl ON cl.oid=c.conrelid JOIN pg_namespace n ON n.oid=cl.relnamespace CROSS JOIN LATERAL unnest(c.conkey) k(attnum) JOIN pg_attribute a ON a.attrelid=cl.oid AND a.attnum=k.attnum WHERE c.contype='f' AND n.nspname='public')
);
